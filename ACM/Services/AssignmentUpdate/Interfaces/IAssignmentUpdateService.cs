using ACM.Models.Dto;

namespace ACM.Services.AssignmentUpdate.Interfaces
{
    public interface IAssignmentUpdateService
    {
        Task RunAsync(CancellationToken cancellationToken = default);

        Task RunWithStatusAsync(
            IReadOnlyList<UAVStatusData> statusData,
            CancellationToken cancellationToken = default
        );
    }
}
