using ACM.Models;
using ACM.Models.Dto;

namespace ACM.Extentions
{
    public static class EntitiyExtentions
    {
        public static Sleeve ToModel(this SleeveDeviceManagerDto sleeveDeviceManagerDto)
        {
            return new Sleeve(
                sleeveDeviceManagerDto.Name,
                sleeveDeviceManagerDto.Location,
                sleeveDeviceManagerDto.PortNumbers
            );
        }
    }
}
