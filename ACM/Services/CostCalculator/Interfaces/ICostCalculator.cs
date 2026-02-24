using ACM.Models;
using Core.Models;

namespace ACM.Services.CostCalculator.Interfaces
{
    public interface ICostCalculator
    {
        int GetCost(Location uavLocation, Sleeve sleeve);
    }
}
