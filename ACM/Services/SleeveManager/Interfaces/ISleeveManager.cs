using ACM.Models;
using ACM.Models.Dto;

namespace ACM.Services.SleeveManager.Interfaces
{
    public interface ISleeveManager
    {
        public void SaveSleevs(IEnumerable<SleeveDeviceManagerDto> sleeves);
        public void DeleteSleeves(DeleteSleeveDto deleteSleeveDto);
        public void UpdateSleeves(UpdateSleeveDto updateSleeveDto);
    }
}
