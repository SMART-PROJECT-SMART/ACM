namespace ACM.Models.Dto
{
    public class AssignSleeveToUavRequestDto
    {
        public AssignSleeveToUavRequestDto(int tailId, string sleeveName)
        {
            TailId = tailId;
            SleeveName = sleeveName;
        }

        public int TailId { get; set; }
        public string SleeveName { get; set; }
    }
}
