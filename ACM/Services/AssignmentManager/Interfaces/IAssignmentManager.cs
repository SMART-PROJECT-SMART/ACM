using ACM.Models;

namespace ACM.Services.AssignmentManager.Interfaces
{
    public interface IAssignmentManager
    {
        Task SetAssignmentAsync(
            IReadOnlyDictionary<int, Sleeve> newAssignment,
            IReadOnlyDictionary<int, int> currentTailToSleeveId,
            CancellationToken cancellationToken = default);
    }
}
