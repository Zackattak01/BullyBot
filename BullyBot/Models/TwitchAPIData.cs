using System;
using Newtonsoft.Json;

namespace BullyBot
{
    public class TwitchAPIData
    {
        [JsonProperty("data")]
        public TwitchData[] Data { get; set; }

        [JsonProperty("pagination")]
        public TwitchPagination Pagination { get; set; }

        public class TwitchData
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("user_id")]
            public string UserId { get; set; }

            [JsonProperty("user_name")]
            public string Username { get; set; }

            [JsonProperty("game_id")]
            public string GameId { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("viewer_count")]
            public int ViewerCount { get; set; }

            [JsonProperty("started_at")]
            public DateTime StartedAt { get; set; }

            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("thumbnail_url")]
            public string ThumbnailUrl { get; set; }

            [JsonProperty("tag_ids")]
            public string[] TagIds { get; set; }
        }

        public class TwitchPagination
        {
            [JsonProperty("cursor")]
            public string Cursor { get; set; }
        }
    }
}