using Core.Common.Enums;

namespace ACM.Models.Dto
{
    public class SleeveChangedNotificationDto
    {
        public SleeveChangedNotificationDto(CrudOperation operation, string name)
        {
            Operation = operation;
            Name = name;
        }

        public CrudOperation Operation { get; set; }
        public string Name { get; set; }
    }
}
