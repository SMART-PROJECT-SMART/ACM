using ACM.Common;
using ACM.Models;
using ACM.Models.Dto;
using ACM.Services.CostCalculator.Interfaces;
using ACM.Services.OptimalAssignmentSolver.Interfaces;
using HungarianAlgorithm;

namespace ACM.Services.OptimalAssignmentSolver
{
    public class OptimalAssignmentSolver : IOptimalAssignmentSolver
    {
        private readonly ICostCalculator _costCalculator;

        public OptimalAssignmentSolver(ICostCalculator costCalculator)
        {
            _costCalculator = costCalculator;
        }

        public Dictionary<int, Sleeve> Solve(
            IReadOnlyList<UAVStatusData> statusList,
            IReadOnlyList<Sleeve> sleeveList
        )
        {
            int rows = statusList.Count;
            int cols = sleeveList.Count;
            int[,] costMatrix = BuildCostMatrix(rows, cols, statusList, sleeveList);
            int[] assignmentIndices =
                HungarianAlgorithm.HungarianAlgorithm.FindAssignments(costMatrix);
            return MapToAssignment(assignmentIndices, statusList, sleeveList, rows, cols);
        }

        private int[,] BuildCostMatrix(
            int rows,
            int cols,
            IReadOnlyList<UAVStatusData> statusList,
            IReadOnlyList<Sleeve> sleeveList
        )
        {
            int n = Math.Max(rows, cols);
            int[,] costMatrix = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i < rows && j < cols)
                    {
                        costMatrix[i, j] = _costCalculator.GetCost(
                            statusList[i].Location,
                            sleeveList[j]
                        );
                    }
                    else
                    {
                        costMatrix[i, j] = ACMConstants.Assignment.DUMMY_COST;
                    }
                }
            }

            return costMatrix;
        }

        private static Dictionary<int, Sleeve> MapToAssignment(
            int[] assignmentIndices,
            IReadOnlyList<UAVStatusData> statusList,
            IReadOnlyList<Sleeve> sleeveList,
            int rows,
            int cols
        )
        {
            Dictionary<int, Sleeve> result = new Dictionary<int, Sleeve>();

            for (int i = 0; i < rows; i++)
            {
                int sleeveIndex = assignmentIndices[i];
                if (sleeveIndex < cols)
                {
                    result[statusList[i].TailId] = sleeveList[sleeveIndex];
                }
            }

            return result;
        }
    }
}
