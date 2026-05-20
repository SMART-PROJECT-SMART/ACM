namespace ACM.Models.Dto
{
    public class AssignSleeveToUavRequestDto
    {
        public AssignSleeveToUavRequestDto(int tailId, int sleeveId)
        {
            TailId = tailId;
            SleeveId = sleeveId;
        }

        public int TailId { get; set; }
        public int SleeveId { get; set; }
    }
}
