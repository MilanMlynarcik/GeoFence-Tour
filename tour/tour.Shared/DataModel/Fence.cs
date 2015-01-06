using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace tour
{
    class Fence
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "fencename")]
        public string FenceName { get; set; }

        [JsonProperty(PropertyName = "fencedescription")]
        public string FenceDescription { get; set; }
        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }
        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }
        [JsonProperty(PropertyName = "tourname")]
        public string TourName { get; set; }


    }
}
