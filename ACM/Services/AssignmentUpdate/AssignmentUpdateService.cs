using ACM.Models.Dto;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.AssignmentUpdate.Interfaces;
using ACM.Services.Kafka.Consumers.StatusConsumer.Interfaces;
using ACM.Services.ScoreCalculator.Interfaces;
using ACM.Services.SleeveManager.Interfaces;
using Core.Models;

namespace ACM.Services.AssignmentUpdate
{
    public class AssignmentUpdateService : IAssignmentUpdateService
    {
        private readonly IUAVStatusConsumer _uavStatusConsumer;
        private readonly ISleeveManager _sleeveManager;
        private readonly IScoreCalculator _scoreCalculator;
        private readonly IAssignmentManager _assignmentManager;

        public AssignmentUpdateService(
            IUAVStatusConsumer uavStatusConsumer,
            ISleeveManager sleeveManager,
            IScoreCalculator scoreCalculator,
            IAssignmentManager assignmentManager)
        {
            _uavStatusConsumer = uavStatusConsumer;
            _sleeveManager = sleeveManager;
            _scoreCalculator = scoreCalculator;
            _assignmentManager = assignmentManager;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<UAVStatusData> statusDataList = _uavStatusConsumer.ConsumeUAVStatus(cancellationToken);
            List<UAVStatusData> statusList = statusDataList.ToList();
            IEnumerable<ACM.Models.Sleeve> sleeves = _sleeveManager.GetAllSleeves();
            List<ACM.Models.Sleeve> sleeveList = sleeves.ToList();

            if (!sleeveList.Any() || !statusList.Any())
            {
                return;
            }

            Dictionary<int, ACM.Models.Sleeve> newAssignment = ComputeBestAssignment(statusList, sleeveList);
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
