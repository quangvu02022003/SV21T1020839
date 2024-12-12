﻿using SV21T1020839.DomainModels;

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
}