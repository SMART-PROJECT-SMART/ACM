using ACM.Models;
using ACM.Models.Dto;

namespace ACM.Services.SleeveManager.Interfaces
{
    public interface ISleeveManager
    {
        IEnumerable<Sleeve> GetAllSleeves();
        void SaveSleevs(IEnumerable<SleeveDeviceManagerDto> sleeves);
        void DeleteSleeves(DeleteSleeveDto deleteSleeveDto);
        void UpdateSleeves(UpdateSleeveDto updateSleeveDto);
    }
}
