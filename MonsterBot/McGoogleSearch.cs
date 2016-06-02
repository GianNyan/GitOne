using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Google
{
    namespace Search
    {
        public class GwebResponse
        {
            public GwebResponseData responseData;
            public string responseDetails;
            public int responseStatus;
        }

        public class GwebResponseData
        {
            public GwebResult[] results;
            public GwebCursor cursor;

        }

        public class GwebCursor
        {
            public string resultCount;
            public GwebPage[] pages;
            public string estimatedResultCount;
            public int currentPageIndex;
            public string moreResultsUrl;
            public string searchResultTime;
        }

        public class GwebPage
        {
            public string start;
            public int label;
        }

        public class GwebResult
        {
            public string GsearchResultClass;
            public string unescapedUrl;
            public string url;
            public string visibleUrl;
            public string cacheUrl;
            public string title;
            public string titleNoFormatting;
            public string content;
        }
    }
}
