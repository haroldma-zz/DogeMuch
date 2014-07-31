using System;
using Newtonsoft.Json;

namespace DogeMuch.Model
{
    public class BlockAddress
    {
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "available_balance")]
        public double Balance { get; set; }
    }
}