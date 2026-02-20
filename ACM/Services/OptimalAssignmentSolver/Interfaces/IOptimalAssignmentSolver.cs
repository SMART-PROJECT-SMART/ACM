using ACM.Models;
using ACM.Models.Dto;

namespace ACM.Services.OptimalAssignmentSolver.Interfaces
{
    public interface IOptimalAssignmentSolver
    {
        Dictionary<int, Sleeve> Solve(
            IReadOnlyList<UAVStatusData> statusList,
            IReadOnlyList<Sleeve> sleeveList
        );
    }
}
