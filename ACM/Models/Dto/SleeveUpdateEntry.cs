using System.ComponentModel.DataAnnotations;

namespace ACM.Models.Dto
{
    public class SleeveUpdateEntry
    {
        [Required]
        public required int Id { get; set; }
        public string? NewName { get; set; }
        public IEnumerable<int>? PortNumbers { get; set; }
    }
}
