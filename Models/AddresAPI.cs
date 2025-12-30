using System.Collections.Generic;
using Newtonsoft.Json;

namespace Laptop88_3.Models
{
    public class Province
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("codename")]
        public string CodeName { get; set; }

        [JsonProperty("division_type")]
        public string DivisionType { get; set; }

        [JsonProperty("phone_code")]
        public int PhoneCode { get; set; }

        [JsonProperty("districts")]
        public List<District> Districts { get; set; } = new List<District>();
    }

    public class District
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("codename")]
        public string CodeName { get; set; }

        [JsonProperty("division_type")]
        public string DivisionType { get; set; }

        [JsonProperty("wards")]
        public List<Ward> Wards { get; set; } = new List<Ward>();
    }

    public class Ward
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("codename")]
        public string CodeName { get; set; }

        [JsonProperty("division_type")]
        public string DivisionType { get; set; }
    }

}
