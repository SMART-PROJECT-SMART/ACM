using ACM.Models;
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
            ILogger<AssignmentUpdateService> logger
        )
        {
            _uavStatusConsumer = uavStatusConsumer;
            _sleeveManager = sleeveManager;
            _scoreCalculator = scoreCalculator;
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

            Dictionary<int, Sleeve> newAssignment = ComputeBestAssignment(statusList, sleeveList);
            _logger.LogInformation(
                "Computed assignment for {Count} UAVs: {Assignment}",
                newAssignment.Count,
                string.Join("; ", newAssignment.Select(kv => $"TailId {kv.Key} -> {kv.Value.Name}"))
            );
            await _assignmentManager.SetAssignmentAsync(newAssignment, cancellationToken);
        }

        private Dictionary<int, Sleeve> ComputeBestAssignment(
            List<UAVStatusData> statusList,
            List<Sleeve> sleeveList
        )
        {
            var assignment = new Dictionary<int, Sleeve>();
            foreach (UAVStatusData status in statusList)
            {
                Sleeve? bestSleeve = null;
                double bestScore = double.MinValue;
                foreach (Sleeve sleeve in sleeveList)
                {
                    double score = _scoreCalculator.GetScore(status.Location, sleeve);
                    if (IsBetterAssignment(score, bestScore, sleeve, bestSleeve))
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

        private static bool IsBetterAssignment(double score, double bestScore, Sleeve candidate, Sleeve? current)
        {
            return score > bestScore
                || (score == bestScore
                    && current != null
                    && string.CompareOrdinal(candidate.Name, current.Name) < 0);
        }
    }
}
