using Core.Common.Enums;

namespace ACM.Models.Dto
{
    public class SleeveChangedNotificationDto
    {
        public CrudOperation Operation { get; set; }
        public string Name { get; set; }

        public SleeveChangedNotificationDto(CrudOperation operation, string name)
        {
            Operation = operation;
            Name = name;
        }
    }
}
