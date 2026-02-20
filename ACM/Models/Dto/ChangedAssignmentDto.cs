namespace ACM.Models.Dto
{
    public class ChangedAssignmentDto
    {
        public int TailId { get; set; }
        public int SleeveId { get; set; }
        public string SleeveName { get; set; } = string.Empty;
        public IEnumerable<int> NewPorts { get; set; } = [];
    }
}
