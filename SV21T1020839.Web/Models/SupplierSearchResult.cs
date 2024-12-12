using SV21T1020839.DomainModels;

namespace SV21T1020839.Web.Models
{
        public class SupplierSearchResult : PaginationSearchResult
        {
            public required List<Supplier> Data { get; set; }
        }
    }

