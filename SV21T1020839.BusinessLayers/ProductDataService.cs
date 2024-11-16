using DataLayers;
using SV21T1020839.DataLayers;
using SV21T1020839.DataLayers.SQLServer;
using SV21T1020839.DomainModels;

namespace SV21T1020839.BusinessLayers
{
    public static class ProductDataService
    {
        private static readonly IProductDAL<Product> productDB;
        private static readonly ICommonDAL<Supplier> supplierDB;
        private static readonly ICommonDAL<Category> categoryDB;

        static ProductDataService()
        {
            string connectionString = Configuration.Connectionstring;
            productDB = new ProductDAL(Configuration.Connectionstring);
            supplierDB = new SupplierDAL(Configuration.Connectionstring);
            categoryDB = new CategoryDAL(connectionString);
        }

        public static List<Supplier> ListOfSuppliers()
        {
            return supplierDB.List();
        }
        public static List<Category> ListOfCategories()
        {
            return categoryDB.List();
        }
        public static List<Product>ListProducts(out int rowCount,int pageSize = 0,int page = 1,
                                                    string searchValue = "",int categoryID = 0, int supplierID = 0,
                                                    decimal minValue = 0, decimal maxValue = 0)
        {
            rowCount = productDB.Count(searchValue);
            return productDB.List( pageSize,page, searchValue, categoryID, supplierID, minValue, maxValue);
                
        }
    }
}
