using System.Collections.Concurrent;
using ACM.Extentions;
using ACM.Models;
using ACM.Models.Dto;
using ACM.Services.SleeveManager.Interfaces;

namespace ACM.Services.SleeveManager
{
    public class SleeveManager : ISleeveManager
    {
        private readonly ConcurrentDictionary<int, Sleeve> _sleevesById;

        public SleeveManager()
        {
            _sleevesById = new ConcurrentDictionary<int, Sleeve>();
        }

        public IReadOnlyList<Sleeve> GetAllSleeves()
        {
            return _sleevesById.Values.ToList();
        }

        public void DeleteSleeves(DeleteSleeveDto deleteSleeveDto)
        {
            foreach (int id in deleteSleeveDto.SleeveIdsToDelete)
            {
                _sleevesById.TryRemove(id, out _);
            }
        }

        public void SaveSleeves(IEnumerable<SleeveDeviceManagerDto> sleeves)
        {
            foreach (SleeveDeviceManagerDto sleeveDto in sleeves)
            {
                Sleeve sleeve = sleeveDto.ToModel();
                _sleevesById[sleeve.Id] = sleeve;
            }
        }

        public void UpdateSleeves(UpdateSleeveDto updateSleeveDto)
        {
            foreach (SleeveUpdateEntry entry in updateSleeveDto.SleevesToUpdate)
            {
                if (_sleevesById.TryGetValue(entry.Id, out Sleeve? sleeve))
                {
                    UpdatePortNumbers(sleeve, entry);
                    UpdateName(sleeve, entry);
                }
            }
        }

        private void UpdatePortNumbers(Sleeve sleeve, SleeveUpdateEntry entry)
        {
            if (entry.PortNumbers is not null)
            {
                sleeve.PortNumbers = entry.PortNumbers;
            }
        }

        private void UpdateName(Sleeve sleeve, SleeveUpdateEntry entry)
        {
            if (entry.NewName is not null && entry.NewName != sleeve.Name)
            {
                sleeve.Name = entry.NewName;
            }
        }
    }
}
