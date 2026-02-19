using ACM.Models.Dto;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.AssignmentUpdate.Interfaces;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using ACM.Services.ScoreCalculator.Interfaces;
using ACM.Services.SleeveManager.Interfaces;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace ACM.Services.AssignmentUpdate
{
    public class AssignmentUpdateService : IAssignmentUpdateService
    {
        private readonly IUAVStatusConsumer _uavStatusConsumer;
        private readonly ISleeveManager _sleeveManager;
        private readonly IScoreCalculator _scoreCalculator;
        private readonly IAssignmentManager _assignmentManager;
        private readonly ILogger<AssignmentUpdateService> _logger;

        public AssignmentUpdateService(
            IUAVStatusConsumer uavStatusConsumer,
            ISleeveManager sleeveManager,
            IScoreCalculator scoreCalculator,
            IAssignmentManager assignmentManager,
            ILogger<AssignmentUpdateService> logger)
        {
            _uavStatusConsumer = uavStatusConsumer;
            _sleeveManager = sleeveManager;
            _scoreCalculator = scoreCalculator;
            _assignmentManager = assignmentManager;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<UAVStatusData> statusDataList = _uavStatusConsumer.ConsumeUAVStatus(cancellationToken);
            List<UAVStatusData> statusList = statusDataList.ToList();

            if (statusList.Count == 0)
            {
                _logger.LogInformation("No UAV status consumed this run (timeout or empty topic)");
            }
            else
            {
                _logger.LogInformation(
                    "Consumed UAV status for {Count} UAVs. TailIds: {TailIds}",
                    statusList.Count,
                    string.Join(", ", statusList.Select(s => s.TailId)));
            }

            IEnumerable<ACM.Models.Sleeve> sleeves = _sleeveManager.GetAllSleeves();
            List<ACM.Models.Sleeve> sleeveList = sleeves.ToList();
            _logger.LogInformation("Sleeves available for assignment: {SleeveCount}. Names: {SleeveNames}", sleeveList.Count, string.Join(", ", sleeveList.Select(s => s.Name)));

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

            Dictionary<int, ACM.Models.Sleeve> newAssignment = ComputeBestAssignment(statusList, sleeveList);
            _logger.LogInformation("Computed assignment for {Count} UAVs: {Assignment}", newAssignment.Count, string.Join("; ", newAssignment.Select(kv => $"TailId {kv.Key} -> {kv.Value.Name}")));
            await _assignmentManager.SetAssignmentAsync(newAssignment, cancellationToken);
        }

        private Dictionary<int, ACM.Models.Sleeve> ComputeBestAssignment(
            List<UAVStatusData> statusList,
            List<ACM.Models.Sleeve> sleeveList)
        {
            var assignment = new Dictionary<int, ACM.Models.Sleeve>();
            foreach (UAVStatusData status in statusList)
            {
                ACM.Models.Sleeve? bestSleeve = null;
                double bestScore = double.MinValue;
                foreach (ACM.Models.Sleeve sleeve in sleeveList)
                {
                    double score = _scoreCalculator.GetScore(status.Location, sleeve);
                    if (score > bestScore ||
                        (score == bestScore && bestSleeve != null && string.CompareOrdinal(sleeve.Name, bestSleeve.Name) < 0))
                    {
                        bestScore = score;
                        bestSleeve = sleeve;
                    }
                }

                if (bestSleeve != null)
                {
                    assignment[status.TailId] = bestSleeve;
                }
            }

            return assignment;
        }
    }
}
