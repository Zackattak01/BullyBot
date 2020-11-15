using System;

namespace BullyBot
{
    class TwitchAPIData
    {
        public Data[] data;
        public Pagination pagination;

        internal struct Data
        {
            public string id;
            public string user_id;
            public string user_name;
            public string game_id;
            public string type;
            public string title;
            public int viewer_count;
            public DateTime started_at;
            public string language;
            public string thumbnail_url;
            public string[] tag_ids;
        }

        internal struct Pagination
        {
            public string cursor;
        }
    }
}