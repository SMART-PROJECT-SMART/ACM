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
        private readonly ConcurrentDictionary<string, int> _sleeveNameToId;

        public SleeveManager()
        {
            _sleevesById = new ConcurrentDictionary<int, Sleeve>();
            _sleeveNameToId = new ConcurrentDictionary<string, int>();
        }

        public IEnumerable<Sleeve> GetAllSleeves()
        {
            return _sleevesById.Values;
        }

        public void DeleteSleeves(DeleteSleeveDto deleteSleeveDto)
        {
            foreach (string sleeveName in deleteSleeveDto.SleevsToDelete)
            {
                if (_sleeveNameToId.TryRemove(sleeveName, out int id))
                {
                    _sleevesById.TryRemove(id, out _);
                }
            }
        }

        public void SaveSleevs(IEnumerable<SleeveDeviceManagerDto> sleeves)
        {
            foreach (SleeveDeviceManagerDto sleeveDto in sleeves)
            {
                Sleeve sleeve = sleeveDto.ToModel();
                _sleevesById[sleeve.Id] = sleeve;
                _sleeveNameToId[sleeve.Name] = sleeve.Id;
            }
        }

        public void UpdateSleeves(UpdateSleeveDto updateSleeveDto)
        {
            foreach (SleeveUpdateEntry entry in updateSleeveDto.SleevesToUpdate)
            {
                if (_sleeveNameToId.TryGetValue(entry.Name, out int id) && _sleevesById.TryGetValue(id, out Sleeve? sleeve))
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
                if (_sleeveNameToId.TryRemove(entry.Name, out int id))
                {
                    sleeve.Name = entry.NewName;
                    _sleeveNameToId[entry.NewName] = id;
                }
            }
        }
    }
}
