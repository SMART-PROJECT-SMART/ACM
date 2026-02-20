using ACM.Models;
using ACM.Models.Dto;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.AssignmentUpdate.Interfaces;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using ACM.Services.OptimalAssignmentSolver.Interfaces;
using ACM.Services.SleeveManager.Interfaces;
using Microsoft.Extensions.Logging;

namespace ACM.Services.AssignmentUpdate
{
    public class AssignmentUpdateService : IAssignmentUpdateService
    {
        private readonly IUAVStatusConsumer _uavStatusConsumer;
        private readonly ISleeveManager _sleeveManager;
        private readonly IOptimalAssignmentSolver _optimalAssignmentSolver;
        private readonly IAssignmentManager _assignmentManager;
        private readonly ILogger<AssignmentUpdateService> _logger;

        public AssignmentUpdateService(
            IUAVStatusConsumer uavStatusConsumer,
            ISleeveManager sleeveManager,
            IOptimalAssignmentSolver optimalAssignmentSolver,
            IAssignmentManager assignmentManager,
            ILogger<AssignmentUpdateService> logger
        )
        {
            _uavStatusConsumer = uavStatusConsumer;
            _sleeveManager = sleeveManager;
            _optimalAssignmentSolver = optimalAssignmentSolver;
            _assignmentManager = assignmentManager;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<UAVStatusData> statusDataList =
                _uavStatusConsumer.ConsumeUAVStatus(cancellationToken);
            List<UAVStatusData> statusList = statusDataList.ToList();
            await RunWithStatusAsync(statusList, cancellationToken);
        }

        public async Task RunWithStatusAsync(
            IReadOnlyList<UAVStatusData> statusData,
            CancellationToken cancellationToken = default
        )
        {
            List<UAVStatusData> statusList = statusData.ToList();

            if (statusList.Count > 0)
            {
                _logger.LogInformation(
                    "UAV status for {Count} UAVs. TailIds: {TailIds}",
                    statusList.Count,
                    string.Join(", ", statusList.Select(s => s.TailId))
                );
            }

            IEnumerable<Sleeve> sleeves = _sleeveManager.GetAllSleeves();
            List<Sleeve> sleeveList = sleeves.ToList();
            _logger.LogInformation(
                "Sleeves available for assignment: {SleeveCount}. Names: {SleeveNames}",
                sleeveList.Count,
                string.Join(", ", sleeveList.Select(s => s.Name))
            );

            if (sleeveList.Count == 0)
            {
                _logger.LogWarning("No sleeves loaded; skipping assignment");
                return;
            }

            if (statusList.Count == 0)
            {
                _logger.LogInformation("No UAV status to assign; skipping");
                return;
            }

            Dictionary<int, Sleeve> newAssignment =
                _optimalAssignmentSolver.Solve(statusList, sleeveList);
            _logger.LogInformation(
                "Computed assignment for {Count} UAVs: {Assignment}",
                newAssignment.Count,
                string.Join("; ", newAssignment.Select(kv => $"TailId {kv.Key} -> {kv.Value.Name}"))
            );

            Dictionary<int, int> currentTailToSleeveId = statusList
                .ToDictionary(s => s.TailId, s => s.SleeveId);

            await _assignmentManager.SetAssignmentAsync(newAssignment, currentTailToSleeveId, cancellationToken);
        }
    }
}
