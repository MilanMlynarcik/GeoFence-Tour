using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace tour
{
    class Tour
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "tourname")]
        public string TourName { get; set; }

        [JsonProperty(PropertyName = "tourdescription")]
        public string TourDescription { get; set; }
    }
}
