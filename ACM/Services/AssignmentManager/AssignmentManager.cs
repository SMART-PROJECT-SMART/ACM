using System.Collections.Concurrent;
using ACM.Models;
using ACM.Models.Dto;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;
using ACM.Services.Clients.SimulatorClient.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACM.Services.AssignmentManager
{
    public class AssignmentManager : IAssignmentManager
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISimulatorClient _simulatorClient;
        private readonly ILogger<AssignmentManager> _logger;
        private readonly ConcurrentDictionary<int, Sleeve> _tailIdToSleeve;

        public AssignmentManager(
            IServiceScopeFactory scopeFactory,
            ISimulatorClient simulatorClient,
            ILogger<AssignmentManager> logger
        )
        {
            _scopeFactory = scopeFactory;
            _simulatorClient = simulatorClient;
            _logger = logger;
            _tailIdToSleeve = new ConcurrentDictionary<int, Sleeve>();
        }

        public IReadOnlyDictionary<int, Sleeve> GetCurrentAssignment()
        {
            return new Dictionary<int, Sleeve>(_tailIdToSleeve);
        }

        public async Task SetAssignmentAsync(
            IReadOnlyDictionary<int, Sleeve> newAssignment,
            CancellationToken cancellationToken = default
        )
        {
            List<ChangedAssignmentDto> changedAssignments = CollectChangedAssignments(
                newAssignment
            );
            HashSet<int> removedTails = _tailIdToSleeve.Keys.Except(newAssignment.Keys).ToHashSet();

            _logger.LogInformation(
                "SetAssignment: {NewCount} UAVs in new assignment, {ChangedCount} changed, {RemovedCount} removed",
                newAssignment.Count,
                changedAssignments.Count,
                removedTails.Count
            );

            if (changedAssignments.Count == 0 && removedTails.Count == 0)
            {
                _logger.LogInformation("No assignment changes; nothing to notify");
                return;
            }

            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                IDeviceManagerClient deviceManagerClient =
                    scope.ServiceProvider.GetRequiredService<IDeviceManagerClient>();
                await deviceManagerClient.ApplyAssignmentChangesAsync(
                    changedAssignments,
                    removedTails,
                    cancellationToken
                );
            }

            UpdateInMemoryAssignment(newAssignment, removedTails);

            foreach (ChangedAssignmentDto change in changedAssignments)
            {
                try
                {
                    await _simulatorClient.NotifyUavPortsChangedAsync(
                        change.TailId,
                        change.NewPorts,
                        cancellationToken
                    );
                    _logger.LogInformation(
                        "Notified simulator of port change for UAV tail {TailId}, sleeve {SleeveName}",
                        change.TailId,
                        change.SleeveName
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to notify simulator of port change for UAV tail {TailId}, sleeve {SleeveName}",
                        change.TailId,
                        change.SleeveName
                    );
                }
            }
        }

        private List<ChangedAssignmentDto> CollectChangedAssignments(
            IReadOnlyDictionary<int, Sleeve> newAssignment
        )
        {
            List<ChangedAssignmentDto> result = new();
            foreach (KeyValuePair<int, Sleeve> kv in newAssignment)
            {
                if (
                    !_tailIdToSleeve.TryGetValue(kv.Key, out Sleeve? currentSleeve)
                    || currentSleeve.Id != kv.Value.Id
                )
                {
                    result.Add(
                        new ChangedAssignmentDto
                        {
                            TailId = kv.Key,
                            SleeveId = kv.Value.Id,
                            SleeveName = kv.Value.Name,
                            NewPorts = kv.Value.PortNumbers,
                        }
                    );
                }
            }

            return result;
        }

        private void UpdateInMemoryAssignment(
            IReadOnlyDictionary<int, Sleeve> newAssignment,
            HashSet<int> removedTails
        )
        {
            foreach (KeyValuePair<int, Sleeve> kv in newAssignment)
            {
                _tailIdToSleeve[kv.Key] = kv.Value;
            }

            foreach (int tailId in removedTails)
            {
                _tailIdToSleeve.TryRemove(tailId, out _);
            }
        }
    }
}
