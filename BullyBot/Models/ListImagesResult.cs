using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BullyBot
{
    public class ListImagesResult
    {
        [JsonProperty("names")]
        public string[] Base64EncodedNames { get; set; }

        public IEnumerable<string> GetDecodedNames()
        {
            foreach (var encodedName in Base64EncodedNames)
            {
                var baseName = encodedName.Split('.', 2).First();

                byte[] data = Convert.FromBase64String(baseName);
                yield return Encoding.UTF8.GetString(data);
            }
        }
    }
}