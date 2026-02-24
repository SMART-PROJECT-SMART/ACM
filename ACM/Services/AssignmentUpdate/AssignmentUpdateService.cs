using ACM.Models;
using ACM.Models.Dto;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.AssignmentUpdate.Interfaces;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using ACM.Services.OptimalAssignmentSolver.Interfaces;
using ACM.Services.SleeveManager.Interfaces;

namespace ACM.Services.AssignmentUpdate
{
    public class AssignmentUpdateService : IAssignmentUpdateService
    {
        private readonly IUAVStatusConsumer _uavStatusConsumer;
        private readonly ISleeveManager _sleeveManager;
        private readonly IOptimalAssignmentSolver _optimalAssignmentSolver;
        private readonly IAssignmentManager _assignmentManager;

        public AssignmentUpdateService(
            IUAVStatusConsumer uavStatusConsumer,
            ISleeveManager sleeveManager,
            IOptimalAssignmentSolver optimalAssignmentSolver,
            IAssignmentManager assignmentManager
        )
        {
            _uavStatusConsumer = uavStatusConsumer;
            _sleeveManager = sleeveManager;
            _optimalAssignmentSolver = optimalAssignmentSolver;
            _assignmentManager = assignmentManager;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<UAVStatusData> statusDataList =
                _uavStatusConsumer.ConsumeUAVStatus(cancellationToken);
            List<UAVStatusData> statusData = statusDataList.ToList();
            await RunWithStatusAsync(statusData, cancellationToken);
        }

        public async Task RunWithStatusAsync(
            IReadOnlyList<UAVStatusData> statusData,
            CancellationToken cancellationToken = default
        )
        {
            IReadOnlyList<Sleeve> sleeves = _sleeveManager.GetAllSleeves();

            if (sleeves.Count == 0 || statusData.Count == 0)
            {
                return;
            }

            Dictionary<int, Sleeve> newAssignment =
                _optimalAssignmentSolver.Solve(statusData, sleeves);

            Dictionary<int, int> currentTailToSleeveId = statusData
                .ToDictionary(s => s.TailId, s => s.SleeveId);

            await _assignmentManager.SetAssignmentAsync(newAssignment, currentTailToSleeveId, cancellationToken);
        }
    }
}
