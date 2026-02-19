using System.Collections.Concurrent;
using ACM.Extentions;
using ACM.Models;
using ACM.Models.Dto;
using ACM.Services.SleeveManager.Interfaces;

namespace ACM.Services.SleeveManager
{
    public class SleeveManager : ISleeveManager
    {
        private readonly ConcurrentDictionary<string, Sleeve> _sleevesByName;

        public SleeveManager()
        {
            _sleevesByName = new ConcurrentDictionary<string, Sleeve>();
        }

        public IEnumerable<Sleeve> GetAllSleeves()
        {
            return _sleevesByName.Values;
        }

        public void DeleteSleeves(DeleteSleeveDto deleteSleeveDto)
        {
            foreach (string sleeveName in deleteSleeveDto.SleevsToDelete)
            {
                _sleevesByName.TryRemove(sleeveName, out _);
            }
        }

        public void SaveSleevs(IEnumerable<SleeveDeviceManagerDto> sleeves)
        {
            foreach (SleeveDeviceManagerDto sleeveDto in sleeves)
            {
                _sleevesByName.TryAdd(sleeveDto.Name, sleeveDto.ToModel());
            }
        }

        public void UpdateSleeves(UpdateSleeveDto updateSleeveDto)
        {
            foreach (SleeveUpdateEntry entry in updateSleeveDto.SleevesToUpdate)
            {
                if (_sleevesByName.TryGetValue(entry.Name, out Sleeve? sleeve))
                {
                    UpdatePortNumbers(sleeve, entry);
                    RenameSleeve(sleeve, entry);
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

        private void RenameSleeve(Sleeve sleeve, SleeveUpdateEntry entry)
        {
            if (entry.NewName is not null && entry.NewName != entry.Name)
            {
                if (_sleevesByName.TryRemove(entry.Name, out _))
                {
                    sleeve.Name = entry.NewName;
                    _sleevesByName.TryAdd(entry.NewName, sleeve);
                }
            }
        }
    }
}
