using ACM.Common;
using ACM.Models;
using ACM.Services.CostCalculator.Interfaces;
using Core.Models;

namespace ACM.Services.CostCalculator
{
    public class DistanceCostCalculator : ICostCalculator
    {
        public int GetCost(Location uavLocation, Sleeve sleeve)
        {
            double latDiff = uavLocation.Latitude - sleeve.Location.Latitude;
            double lonDiff = uavLocation.Longitude - sleeve.Location.Longitude;
            double horizontalDistance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
            return (int)(horizontalDistance * ACMConstants.Assignment.COST_SCALE_FACTOR);
        }
    }
}
