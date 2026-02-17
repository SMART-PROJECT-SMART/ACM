using System.ComponentModel.DataAnnotations;
using Core.Models;

namespace ACM.Models.Dto
{
    public class SleeveDeviceManagerDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public Location Location { get; set; }

        [Required]
        public IEnumerable<int> PortNumbers { get; set; }
    }
}
