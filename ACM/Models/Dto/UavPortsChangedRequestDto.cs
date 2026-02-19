namespace ACM.Models.Dto
{
    public class UavPortsChangedRequestDto
    {
        public int TailId { get; set; }
        public IEnumerable<int> NewPorts { get; set; } = [];
    }
}
