namespace ACM.Models
{
    public class SleeveToTailIdAssignment
    {
        public SleeveToTailIdAssignment(
            IEnumerable<KeyValuePair<int, Sleeve>> tailIdToSleeveAssignemnt
        )
        {
            TailIdToSleeveAssignment = tailIdToSleeveAssignemnt;
        }

        IEnumerable<KeyValuePair<int, Sleeve>> TailIdToSleeveAssignment;
    }
}
