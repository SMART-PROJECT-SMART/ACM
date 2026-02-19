using ACM.Models;

namespace ACM.Services.AssignmentManager.Interfaces
{
    public interface IAssignmentManager
    {
        IReadOnlyDictionary<int, Sleeve> GetCurrentAssignment();
        Task SetAssignmentAsync(
            IReadOnlyDictionary<int, Sleeve> newAssignment,
            CancellationToken cancellationToken = default);
    }
}
