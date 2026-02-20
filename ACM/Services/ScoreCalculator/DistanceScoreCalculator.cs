using ACM.Common;
using ACM.Models;
using ACM.Services.ScoreCalculator.Interfaces;
using Core.Models;

namespace ACM.Services.ScoreCalculator
{
    public class DistanceScoreCalculator : IScoreCalculator
    {
        public int GetScore(Location uavLocation, Sleeve sleeve)
        {
            double distance = uavLocation.CalculateDistanceTo(sleeve.Location);
            double denominator = ACMConstants.Scoring.MIN_DISTANCE_DENOMINATOR + distance;
            double score = 1.0 / denominator;
            return (int)(score * ACMConstants.Scoring.SCORE_MAX_VALUE);
        }
    }
}
