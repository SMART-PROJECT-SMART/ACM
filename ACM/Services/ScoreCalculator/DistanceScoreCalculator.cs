using ACM.Models;
using ACM.Services.CostCalculator.Interfaces;
using Core.Models;

namespace ACM.Services.CostCalculator
{
    public class DistanceCostCalculator : ICostCalculator
    {
        public int GetCost(Location uavLocation, Sleeve sleeve)
        {
            return (int)uavLocation.CalculateDistanceTo(sleeve.Location);
        }
    }
}
