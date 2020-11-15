using System;

namespace BullyBot
{
    class GoogleResults
    {
        public Item[] items;

        internal struct Item
        {
            public string link;
            public string title;
            public PageMap pagemap;
        }

        internal struct PageMap
        {
            public CSE_Thumbnail[] cse_thumbnail;
        }

        internal struct CSE_Thumbnail
        {
            public string src;
        }
    }
}