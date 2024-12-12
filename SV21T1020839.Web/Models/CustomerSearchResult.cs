using SV21T1020839.DomainModels;

namespace SV21T1020839.Web.Models
{
        public class CustomerSearchResult : PaginationSearchResult
        {
            public required List<Customer> Data { get; set; }

        }
    }

