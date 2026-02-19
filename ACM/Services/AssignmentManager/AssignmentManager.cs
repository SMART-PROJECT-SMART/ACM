using System.Collections.Concurrent;
using ACM.Models;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.SimulatorClient.Interfaces;

namespace ACM.Services.AssignmentManager
{
    public class AssignmentManager : IAssignmentManager
    {
        private readonly ISimulatorClient _simulatorClient;
        private readonly ConcurrentDictionary<int, Sleeve> _tailIdToSleeve;

        public AssignmentManager(ISimulatorClient simulatorClient)
        {
            _simulatorClient = simulatorClient;
            _tailIdToSleeve = new ConcurrentDictionary<int, Sleeve>();
        }

        public IReadOnlyDictionary<int, Sleeve> GetCurrentAssignment()
        {
            return new Dictionary<int, Sleeve>(_tailIdToSleeve);
        }

        public async Task SetAssignmentAsync(
            IReadOnlyDictionary<int, Sleeve> newAssignment,
            CancellationToken cancellationToken = default)
        {
            List<(int TailId, IEnumerable<int> NewPorts)> toNotify = new();
            foreach (KeyValuePair<int, Sleeve> kv in newAssignment)
            {
                int tailId = kv.Key;
                Sleeve newSleeve = kv.Value;
                if (!_tailIdToSleeve.TryGetValue(tailId, out Sleeve? currentSleeve) ||
                    currentSleeve.Name != newSleeve.Name)
                {
                    toNotify.Add((tailId, newSleeve.PortNumbers));
                }
            }

            foreach (KeyValuePair<int, Sleeve> kv in newAssignment)
            {
                _tailIdToSleeve[kv.Key] = kv.Value;
            }

            HashSet<int> removedTails = _tailIdToSleeve.Keys.Except(newAssignment.Keys).ToHashSet();
            foreach (int tailId in removedTails)
            {
                _tailIdToSleeve.TryRemove(tailId, out _);
            }

            foreach ((int tailId, IEnumerable<int> newPorts) in toNotify)
            {
                await _simulatorClient.NotifyUavPortsChangedAsync(tailId, newPorts, cancellationToken);
            }
        }
    }
}
