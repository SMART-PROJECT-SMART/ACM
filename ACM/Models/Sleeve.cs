using Core.Models;

namespace ACM.Models
{
    public class Sleeve
    {
        public Sleeve(int id, string name, Location location, IEnumerable<int> portNumbers)
        {
            Id = id;
            Name = name;
            Location = location;
            PortNumbers = portNumbers;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Location Location { get; set; }
        public IEnumerable<int> PortNumbers { get; set; }
    }
}
