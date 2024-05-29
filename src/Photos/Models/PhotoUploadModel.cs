using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photos.Models
{
    public class PhotoUploadModel
    {
        [JsonProperty]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("analysis")]
        public Analysis Analysis { get; set; }
    }

    public class Analysis
    {
        public Metadata Metadata { get; set; }
        public string[] Categories { get; set; }
    }

    public class Metadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }
    }
}
