using Core.Models;

namespace ACM.Models
{
    public class Sleeve
    {
        public Sleeve(string name, Location location, IEnumerable<int> portNumbers)
        {
            Name = name;
            Location = location;
            PortNumbers = portNumbers;
        }

        public string Name { get; set; }
        public Location Location { get; set; }
        public IEnumerable<int> PortNumbers { get; set; }
    }
}
