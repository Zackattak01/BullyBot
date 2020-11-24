using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BullyBot
{


    public class GoogleSearchService
    {
        private readonly string googleKey;

        private readonly string googleCX;

        HttpClient client;
        public GoogleSearchService(HttpClient httpClient)
        {
            client = httpClient;
            googleKey = Environment.GetEnvironmentVariable("GoogleKey");
            googleCX = Environment.GetEnvironmentVariable("GoogleCX");
        }

        public async Task<GoogleResults> SearchAsync(string searchQuery)
        {
            string url = $"https://www.googleapis.com/customsearch/v1?key={googleKey}&cx={googleCX}&items=(link, title, pagemap/cse_thumbnail/src)&num=3&q=" + searchQuery;
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            //decodes the json as a GoogleResults type
            string jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GoogleResults>(jsonString);
        }
    }
}