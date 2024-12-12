using SV21T1020839.DomainModels;

namespace SV21T1020839.Web.Models
{
        public class CategorySearchResult : PaginationSearchResult
        {
            public required List<Category> Data { get; set; }
        }
    }


