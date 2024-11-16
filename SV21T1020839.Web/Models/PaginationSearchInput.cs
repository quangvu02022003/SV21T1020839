using SV21T1020839.DomainModels;

namespace SV21T1020839.Web.Models
{
    public class PaginationSearchInput
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValue { get; set; } = "";
    }

    public class ProductSearchInput : PaginationSearchInput
    {
        public int CategoryID { get; set; } =0;
        public int  SupplierID { get; set; } = 0;
        public decimal minPrice { get; set; } = 0;
        public decimal maxPrice { get; set; } = 0;
    }
}
