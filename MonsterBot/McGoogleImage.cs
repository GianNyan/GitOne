using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Google
{
    namespace Image
    {
        public class GimResponse
        {
            public GimResponseData responseData;
            public string responseDetails;
            public int responseStatus;
        }

        public class GimResponseData
        {
            public GimResult[] results;
            public GimCursor cursor;

        }

        public class GimCursor
        {
            public string resultCount;
            public GimPage[] pages;
            public string estimatedResultCount;
            public int currentPageIndex;
            public string moreResultsUrl;
            public string searchResultTime;
        }

        public class GimPage
        {
            public string start;
            public int label;
        }

        public class GimResult
        {
            public string GsearchResultClass;
            public int width;
            public int height;
            public string imageId;
            public int tbWidth;
            public int tbHeight;
            public string unescapedUrl;
            public string url;
            public string visibleUrl;
            public string title;
            public string titleNoFormatting;
            public string originalContextUrl;
            public string content;
            public string contentNoFormatting;
            public string tbUrl;
        }
    }
}
