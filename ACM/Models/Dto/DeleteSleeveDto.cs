using System.ComponentModel.DataAnnotations;

namespace ACM.Models.Dto
{
    public class DeleteSleeveDto {
        [Required]
        public IEnumerable<string> SleevsToDelete { get; set} 
    }
}
