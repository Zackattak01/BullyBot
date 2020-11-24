using System;

namespace BullyBot
{
    public class GoogleResults
    {
        public Item[] items;

        public struct Item
        {
            public string link;
            public string title;
            public PageMap pagemap;
        }

        public struct PageMap
        {
            public CSE_Thumbnail[] cse_thumbnail;
        }

        public struct CSE_Thumbnail
        {
            public string src;
        }
    }
}