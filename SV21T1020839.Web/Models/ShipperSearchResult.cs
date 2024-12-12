using SV21T1020839.DomainModels;

namespace SV21T1020839.Web.Models
{

        public class ShipperSearchResult : PaginationSearchResult
        {
            public required List<Shipper> Data { get; set; }
        }
    }
