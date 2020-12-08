using System;
using Newtonsoft.Json;

namespace BullyBot
{
    public class GoogleResults
    {
        [JsonProperty("items")]
        public Item[] Items { get; set; }

        public class Item
        {
            [JsonProperty("link")]
            public string Link { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("pagemap")]
            public PageMap PageMap { get; set; }
        }

        public class PageMap
        {
            [JsonProperty("cse_thumbnail")]
            public CSE_Thumbnail[] CseThumbnail { get; set; }
        }

        public class CSE_Thumbnail
        {
            [JsonProperty("src")]
            public string Src { get; set; }
        }
    }
}