using Dapper;
using DataLayers.SQLServer;
using SV21T1020839.DomainModels;
using System.Data;

namespace SV21T1020839.DataLayers.SQLServer
{
    public class ProductDAL : BaseDAL, IProductDAL<Product>
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {

        }

        public int Add(Product data)
        {
            int id = 0;
            using (var connection = Openconnection())
            {
                var sql = @"
                            IF EXISTS (SELECT * FROM Products WHERE ProductName = @ProductName)
                            SELECT -1; 
                            ELSE
                            BEGIN 
                                INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                                VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
                                SELECT CAST(SCOPE_IDENTITY() AS INT); 
                            END";

                var parameter = new
                {
                    ProductName = data.ProductName ?? "",
                    ProductDescription = data.ProductDescription ?? "",
                    SupplierID = data.SupplierID,
                    CategoryID = data.CategoryID,
                    Unit = data.Unit ?? "",
                    Price = data.Price,
                    Photo = data.Photo ?? "",
                    IsSelling = data.IsSelling
                };

                id = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;

        }

        public long AddAttribute(ProductAttribute data)
        {
            long id = 0;
            using (var connection = Openconnection())
            {
                var sql = @"
                            INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue, DisplayOrder)
                            VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                var parameter = new
                {
                    ProductID = data.ProductId,
                    AttributeName = data.AttributeName ?? "",
                    AttributeValue = data.AttributeValue ?? "",
                    DisplayOrder = data.DisplayOrder
                };

                id = connection.ExecuteScalar<long>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }

        public long AddPhoto(ProductPhoto data)
        {
            long id = 0;
            using (var connection = Openconnection())
            {
                var sql = @"
                            INSERT INTO ProductPhotos (ProductID, Photo, Description, DisplayOrder, IsHidden)
                            VALUES (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                var parameter = new
                {
                    ProductID = data.ProductID,
                    Photo = data.Photo ?? "", // Đường dẫn ảnh
                    Description = data.Description ?? "", // Mô tả ảnh
                    DisplayOrder = data.DisplayOrder, // Thứ tự hiển thị
                    IsHidden = data.IsHidden // Trạng thái ẩn
                };

                id = connection.ExecuteScalar<long>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }

        public int Count(string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            int count = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = Openconnection())
            {
                var sql = @"
                            SELECT COUNT(*)
                            FROM Products
                            WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                              AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                              AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                              AND (Price >= @MinPrice)
                              AND (@MaxPrice <= 0 OR Price <= @MaxPrice)";

                var parameters = new
                {
                    SearchValue = searchValue,
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                count = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
            return count;
        }

        public bool Delete(int productID)
        {
            bool result = false;
            using (var connection = Openconnection())
            {
                var sql = @"DELETE FROM Products WHERE ProductID = @ProductID";
                var parameter = new
                {
                    ProductID = productID
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public bool DeleteAttribute(long attributeID)
        {
            bool result = false;
            using (var connection = Openconnection())
            {
                var sql = @"DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID";
                var parameter = new
                {
                    AttributeID = attributeID
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public bool DeletePhoto(long photoID)
        {
            bool result = false;
            using (var connection = Openconnection())
            {
                var sql = @"DELETE FROM ProductPhotos WHERE PhotoID = @PhotoID";
                var parameter = new
                {
                    PhotoID = photoID
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public Product? Get(int productID)
        {
            throw new NotImplementedException();
        }

        public ProductAttribute? GetAttribute(long attributeID)
        {
            throw new NotImplementedException();
        }

        public ProductPhoto? GetPhoto(long photoID)
        {
            throw new NotImplementedException();
        }

        public bool InUsed(int productID)
        {
            bool result = false;
            using (var connection = Openconnection())
            {
                var sql = @"IF EXISTS (SELECT * FROM OrderDetails WHERE ProductID = @ProductID)
                                SELECT 1
                            ELSE
                                SELECT 0";
                var parameter = new
                {
                    ProductID = productID
                };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return result;
        }

        public List<Product> List(int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            List<Product> data = new List<Product>();
            searchValue = $"%{searchValue}%";

            using (var connection = Openconnection())
            {
                var sql = @"
                   select * 
                    from    (
	                            select *, 
	                            row_number() over(order by ProductName) as RowNumber
	                            from Products
	                            where (ProductName like @searchValue)
                            )as t
                    where (@pageSize = 0 or (RowNumber between (@page - 1) * @pageSize + 1 and @page * @pageSize))
                    order by RowNumber ";

                var parameters = new
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchValue = searchValue,
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };

                data = connection.Query<Product>(sql: sql, param: parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return data;
            /*List<Product> data = new List<Product>();
            searchValue = $"%{searchValue}%";
            using (var connection = Openconnection())
            {
                var sql = @"SELECT *
                                FROM (
                                SELECT *,
                                ROW_NUMBER() OVER(ORDER BY ProductName) AS RowNumber
                                FROM Products
                                WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                                AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                                AND (@SupplierID = 0 OR SupplierId = @SupplierID)
                                AND (Price >= @MinPrice)
                                AND (@MaxPrice <= 0 OR Price <= @MaxPrice)
                                ) AS t
                                WHERE (@PageSize = 0)
                                OR (RowNumber BETWEEN (@Page - 1)*@PageSize + 1 AND @Page * @PageSize);";
                var parameters = new
                {
                    page = page,
                    pageSize = pageSize,
                    searchValue = searchValue,
                    categoryID = categoryID,
                    supplierID = supplierID,
                    minPrice = minPrice,
                    maxPrice = maxPrice
                    //ben trai la ten tham so trong cau lenh sql, ben phai la value truyen cho tham so
                };
                data = connection.Query<Product>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text).ToList();
            }
            return data;*/
        }

        public IList<ProductAttribute> ListAttributes(int productID)
        {
            IList<ProductAttribute> data = new List<ProductAttribute>();

            using (var connection = Openconnection())
            {
                var sql = @"SELECT * 
                    FROM ProductAttribute 
                    WHERE ProductID = @ProductID 
                    ORDER BY DisplayOrder";

                var parameter = new
                {
                    ProductID = productID
                };

                data = connection.Query<ProductAttribute>(sql: sql, param: parameter, commandType: CommandType.Text).ToList();
                connection.Close();
            }

            return data;
        }

        public IList<ProductPhoto> ListPhotos(int productID)
        {
            IList<ProductPhoto> data = new List<ProductPhoto>();

            using (var connection = Openconnection())
            {
                var sql = @"SELECT * 
                    FROM ProductPhoto 
                    WHERE ProductID = @ProductID 
                    ORDER BY DisplayOrder";

                var parameter = new
                {
                    ProductID = productID
                };

                data = connection.Query<ProductPhoto>(sql: sql, param: parameter, commandType: CommandType.Text).ToList();
                connection.Close();
            }

            return data;
        }

        public bool Update(Product data)
        {
            bool result = false;

            using (var connection = Openconnection())
            {
                var sql = @"IF NOT EXISTS (SELECT * FROM Products WHERE ProductID <> @ProductID AND ProductName = @ProductName)
                            BEGIN 
                                UPDATE Products
                                SET ProductDescription = @ProductDescription,
                                    SupplierID = @SupplierID,
                                    CategoryID = @CategoryID,
                                    Unit = @Unit,
                                    Price = @Price,
                                    Photo = @Photo,
                                    IsSelling = @IsSelling
                                WHERE ProductID = @ProductID
                            END";

                var parameter = new
                {
                    ProductID = data.ProductID,
                    ProductName = data.ProductName ?? "",
                    ProductDescription = data.ProductDescription ?? "",
                    SupplierID = data.SupplierID,
                    CategoryID = data.CategoryID,
                    Unit = data.Unit ?? "",
                    Price = data.Price,
                    Photo = data.Photo ?? "",
                    IsSelling = data.IsSelling
                };

                result = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public bool UpdateAttribute(ProductAttribute data)
        {
            bool result = false;

            using (var connection = Openconnection())
            {
                var sql = @"IF NOT EXISTS (SELECT * FROM ProductAttributes WHERE AttributeID <> @AttributeID AND ProductID = @ProductID AND AttributeName = @AttributeName)
                    BEGIN 
                        UPDATE ProductAttributes
                        SET AttributeValue = @AttributeValue,
                            DisplayOrder = @DisplayOrder
                        WHERE AttributeID = @AttributeID
                    END";

                var parameter = new
                {
                    AttributeID = data.AttributeId,
                    ProductID = data.ProductId,
                    AttributeName = data.AttributeName ?? "",
                    AttributeValue = data.AttributeValue ?? "",
                    DisplayOrder = data.DisplayOrder
                };

                result = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }

        public bool UpdatePhoto(ProductPhoto data)
        {
            bool result = false;

            using (var connection = Openconnection())
            {
                var sql = @"IF NOT EXISTS (SELECT * FROM ProductPhotos WHERE PhotoID <> @PhotoID AND ProductID = @ProductID AND Description = @Description)
                    BEGIN 
                        UPDATE ProductPhotos
                        SET DisplayOrder = @DisplayOrder,
                            IsHidden = @IsHidden
                        WHERE PhotoID = @PhotoID
                    END";

                var parameter = new
                {
                    PhotoID = data.PhotoID,
                    ProductID = data.ProductID,
                    Description = data.Description ?? "",
                    DisplayOrder = data.DisplayOrder,
                    IsHidden = data.IsHidden
                };

                result = connection.Execute(sql: sql, param: parameter, commandType: CommandType.Text) > 0;
                connection.Close();
            }

            return result;
        }
    }
}
