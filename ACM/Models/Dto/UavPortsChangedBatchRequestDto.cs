namespace ACM.Models.Dto
{
    public class UavPortsChangedBatchRequestDto
    {
        public IEnumerable<UavPortsChangedRequestDto> Changes { get; set; } = [];
    }
}
