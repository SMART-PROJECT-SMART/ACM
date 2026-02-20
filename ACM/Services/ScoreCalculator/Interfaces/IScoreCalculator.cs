using ACM.Models;
using Core.Models;

namespace ACM.Services.ScoreCalculator.Interfaces
{
    public interface IScoreCalculator
    {
        int GetScore(Location uavLocation, Sleeve sleeve);
    }
}
