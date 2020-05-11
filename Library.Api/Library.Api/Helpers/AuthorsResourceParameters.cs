using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Api.Helpers
{
    public class AuthorsResourceParameters
    {
        const int maxPageSize = 20;

        //Paging
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get { return _pageSize; }

            set {_pageSize=(value>maxPageSize)?maxPageSize:value; }
        }

        //Filering
        public string Genre { get; set; }

        //Searching
        public string SearchQuery { get; set; }

        //Sorting 
        public string OrderBy { get; set; } = "Name";

        //Shaping Resource
        public string Fields { get; set; }
    }
}
