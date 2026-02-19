using ACM.Models;
using Core.Models;

namespace ACM.Services.ScoreCalculator.Interfaces
{
    public interface IScoreCalculator
    {
        double GetScore(Location uavLocation, Sleeve sleeve);
    }
}
