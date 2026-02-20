using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Core.Models;

namespace ACM.Models.Dto
{
    public class SleeveDeviceManagerDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [Required]
        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [Required]
        [JsonPropertyName("portNumbers")]
        public IEnumerable<int> PortNumbers { get; set; }
    }
}
