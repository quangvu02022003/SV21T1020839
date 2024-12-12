using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1020839.BusinessLayers;
using SV21T1020839.DomainModels;
using SV21T1020839.Web;
using SV21T1020839.Web.AppCodes;
using SV21T1020839.Web.Models;
using System.Text.RegularExpressions;

namespace SV21T1020839.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.ADMIN},{WebUserRoles.MANAGER}")]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 30;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";

        public IActionResult Index()
        {
            // Lấy điều kiện tìm kiếm từ session hoặc khởi tạo mặc định
            var condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION)
                            ?? new ProductSearchInput
                            {
                                Page = 1,
                                PageSize = PAGE_SIZE,
                                SearchValue = string.Empty
                            };

            condition.Categories = CommonDataService.ListOfCategories(out _, 1, 0, "");
            condition.Suppliers = CommonDataService.ListOfSuppliers(out _, 1, 0, "");

            return View(condition);
        }

        public IActionResult Search(ProductSearchInput condition)
        {
            // Xử lý giá trị đầu vào
            if (!int.TryParse(Regex.Replace(condition.MinPrice, "[,.]", ""), out int minPrice))
                minPrice = 0;

            if (!int.TryParse(Regex.Replace(condition.MaxPrice, "[,.]", ""), out int maxPrice))
                maxPrice = int.MaxValue;

            var data = ProductDataService.ListProducts(out int rowCount, condition.Page, condition.PageSize,
                                                        condition.SearchValue ?? "", condition.CategoryID,
                                                        condition.SupplierID, minPrice, maxPrice);

            var model = new ProductsSearchResult
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);
            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung Mặt hàng";
            return View("Edit", new Product { ProductID = 0, IsSelling = true });
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin Mặt hàng";
            var data = ProductDataService.GetProduct(id);
            if (data == null) return RedirectToAction("Index");
            return View(data);
        }

        [HttpPost]
        public IActionResult Save(Product data, IFormFile uploadPhoto)
        {
            ViewBag.Title = data.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";

            // Kiểm tra dữ liệu đầu vào
            ValidateProduct(data);
            if (!ModelState.IsValid)
                return View("Edit", data);

            // Xử lý upload ảnh
            if (uploadPhoto != null)
                data.Photo = UploadPhoto(uploadPhoto);

            // Thêm mới hoặc cập nhật
            if (data.ProductID == 0)
            {
                ProductDataService.AddProduct(data);
            }
            else
            {
                ProductDataService.UpdateProduct(data);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }

            var data = ProductDataService.GetProduct(id);
            if (data == null) return RedirectToAction("Index");
            return View(data);
        }

        public IActionResult Photo(int id = 0, string method = "", int photoId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    ProductPhoto data = new() { ProductID = id, PhotoID = 0, DisplayOrder = 1, IsHidden = false };
                    return View(data);

                case "edit":
                    ViewBag.Title = "Thay đổi ảnh của mặt hàng";
                    ProductPhoto? productPhoto = ProductDataService.GetPhoto(photoId);
                    if (productPhoto == null)
                    {
                        return RedirectToAction("Edit", new { id = id });
                    }
                    return View(productPhoto);

                case "delete":
                    ProductDataService.DeletePhoto(photoId);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult SavePhoto(ProductPhoto productPhoto, IFormFile uploadPhoto)
        {
            ViewBag.Title = productPhoto.PhotoID == 0 ? "Bổ sung ảnh của mặt hàng" : "Thay đổi ảnh của mặt hàng";

            // Kiểm tra dữ liệu đầu vào
            ValidatePhoto(productPhoto, uploadPhoto);
            if (!ModelState.IsValid)
                return View("Photo", productPhoto);

            // Upload ảnh nếu có
            if (uploadPhoto != null)
                productPhoto.Photo = UploadPhoto(uploadPhoto);

            // Thêm mới hoặc cập nhật
            SaveOrUpdatePhoto(productPhoto);

            return RedirectToAction("Edit", new { id = productPhoto.ProductID });
        }

        public IActionResult SaveAttribute(ProductAttribute productAttribute)
        {
            ViewBag.Title = productAttribute.AttributeID == 0 ? "Bổ sung thuộc tính của mặt hàng" : "Thay đổi thuộc tính của mặt hàng";

            // Kiểm tra dữ liệu đầu vào
            ValidateAttribute(productAttribute);
            if (!ModelState.IsValid)
                return View("Attribute", productAttribute);

            // Thêm mới hoặc cập nhật
            SaveOrUpdateAttribute(productAttribute);

            return RedirectToAction("Edit", new { id = productAttribute.ProductID });
        }

        #region Private Methods

        private void ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                ModelState.AddModelError(nameof(product.ProductName), "Vui lòng nhập tên sản phẩm");

            if (string.IsNullOrWhiteSpace(product.ProductDescription))
                ModelState.AddModelError(nameof(product.ProductDescription), "Vui lòng nhập mô tả của sản phẩm");

            if (string.IsNullOrWhiteSpace(product.Unit))
                ModelState.AddModelError(nameof(product.Unit), "Vui lòng nhập đơn vị tính của sản phẩm");

            if (product.Price <= 0)
                ModelState.AddModelError(nameof(product.Price), "Giá của sản phẩm phải lớn hơn 0");
        }

        private void ValidatePhoto(ProductPhoto photo, IFormFile uploadPhoto)
        {
            if (photo.PhotoID == 0 && uploadPhoto == null)
                ModelState.AddModelError(nameof(photo.Photo), "Vui lòng chọn ảnh của mặt hàng");

            if (string.IsNullOrWhiteSpace(photo.Description))
                ModelState.AddModelError(nameof(photo.Description), "Vui lòng nhập mô tả của sản phẩm");

            if (photo.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(photo.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");
        }

        private void ValidateAttribute(ProductAttribute attribute)
        {
            if (string.IsNullOrWhiteSpace(attribute.AttributeName))
                ModelState.AddModelError(nameof(attribute.AttributeName), "Tên thuộc tính không được để trống");

            if (string.IsNullOrWhiteSpace(attribute.AttributeValue))
                ModelState.AddModelError(nameof(attribute.AttributeValue), "Giá trị thuộc tính không được để trống");

            if (attribute.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(attribute.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0");
        }

        private string UploadPhoto(IFormFile uploadPhoto)
        {
            string fileName = $"{DateTime.Now.Ticks}-{uploadPhoto.FileName}";
            string filePath = Path.Combine(ApplicationContext.WebRootPath, @"images\products", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                uploadPhoto.CopyTo(stream);
            }

            return fileName;
        }

        private void SaveOrUpdatePhoto(ProductPhoto photo)
        {
            if (photo.PhotoID == 0)
                ProductDataService.AddPhoto(photo);
            else
                ProductDataService.UpdatePhoto(photo);
        }

        private void SaveOrUpdateAttribute(ProductAttribute attribute)
        {
            if (attribute.AttributeID == 0)
                ProductDataService.AddAttribute(attribute);
            else
                ProductDataService.UpdateAttribute(attribute);
        }

        private IActionResult HandlePhotoDelete(int productId, int photoId)
        {
            ProductDataService.DeletePhoto(photoId);
            return RedirectToAction("Edit", new { id = productId });
        }

        #endregion
    }
}
