using System.ComponentModel.DataAnnotations;

namespace ACM.Models.Dto
{
    public class UpdateSleeveDto
    {
        [Required]
        public required IEnumerable<SleeveUpdateEntry> SleevesToUpdate { get; set; }
    }
}
