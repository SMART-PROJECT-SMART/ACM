namespace ACM.Services.AssignmentUpdate.Interfaces
{
    public interface IAssignmentUpdateService
    {
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}
