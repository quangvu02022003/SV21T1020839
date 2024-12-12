using SV21T1020839.DomainModels;

namespace SV21T1020839.Web.Models
{
    public class PaginationSearchInput
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValue { get; set; } = "";
    }

 
}
