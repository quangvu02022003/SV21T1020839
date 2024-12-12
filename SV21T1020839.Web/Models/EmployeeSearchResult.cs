using SV21T1020839.DomainModels;

namespace SV21T1020839.Web.Models
{

        public class EmployeeSearchResult : PaginationSearchResult
        {
            public required List<Employee> Data { get; set; }
        }
    }

