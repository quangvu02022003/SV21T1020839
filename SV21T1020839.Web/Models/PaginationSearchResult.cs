using SV21T1020839.DomainModels;

namespace SV21T1020839.Web.Models
{
    public abstract class PaginationSearchResult
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; }

        public string SearchValue { get; set; } = "";
        public int RowCount { get; set; }
        public int PageCount
        {
            get
            {
                if (PageSize == 0)
                    return 1;
                int c = RowCount / PageSize;
                if (RowCount % PageSize > 0)
                    c += 1;
                return c;
            }
        }
    }

    public class CustomerSearchResult : PaginationSearchResult
    {
        public required List<Customer>Data { get; set; }
    }

    public class CategorySearchResult : PaginationSearchResult
    {
        public required List<Category> Data { get; set; }
    }

    public class ShipperSearchResult : PaginationSearchResult
    {
        public required List<Shipper> Data { get; set; }
    }

    public class EmployeeSearchResult : PaginationSearchResult
    {
        public required List<Employee> Data { get; set; }
    }

    public class SupplierSearchResult : PaginationSearchResult
    {
        public required List<Supplier> Data { get; set; }
    }

   

    public class ProductSearchResult : PaginationSearchResult
    {
        public int CategoryID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public decimal minPrice { get; set; } = 0;
        public decimal maxPrice { get; set; } = 0;

        public required List<Product> Data { get; set; }
    }
}

