using ACM.Common;
using ACM.Models;
using ACM.Services.ScoreCalculator.Interfaces;
using Core.Models;

namespace ACM.Services.ScoreCalculator
{
    public class DistanceScoreCalculator : IScoreCalculator
    {
        public double GetScore(Location uavLocation, Sleeve sleeve)
        {
            double distance = uavLocation.CalculateDistanceTo(sleeve.Location);
            double denominator = ACMConstants.Scoring.MIN_DISTANCE_DENOMINATOR + distance;
            return 1.0 / denominator;
        }
    }
}
