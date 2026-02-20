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

        public AssignmentManager(
            IServiceScopeFactory scopeFactory,
            ISimulatorClient simulatorClient,
            ILogger<AssignmentManager> logger
        )
        {
            _scopeFactory = scopeFactory;
            _simulatorClient = simulatorClient;
            _logger = logger;
        }

        public async Task SetAssignmentAsync(
            IReadOnlyDictionary<int, Sleeve> newAssignment,
            IReadOnlyDictionary<int, int> currentTailToSleeveId,
            CancellationToken cancellationToken = default
        )
        {
            List<ChangedAssignmentDto> changedAssignments = CollectChangedAssignments(
                newAssignment,
                currentTailToSleeveId
            );

            _logger.LogInformation(
                "SetAssignment: {NewCount} UAVs in new assignment, {ChangedCount} changed",
                newAssignment.Count,
                changedAssignments.Count
            );

            if (changedAssignments.Count == 0)
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
                    new HashSet<int>(),
                    cancellationToken
                );
            }

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
            IReadOnlyDictionary<int, Sleeve> newAssignment,
            IReadOnlyDictionary<int, int> currentTailToSleeveId
        )
        {
            List<ChangedAssignmentDto> result = new();
            foreach (KeyValuePair<int, Sleeve> kv in newAssignment)
            {
                if (kv.Value.Id == 0)
                {
                    _logger.LogWarning(
                        "Skipping assignment for tail {TailId}: sleeve {SleeveName} has invalid Id 0",
                        kv.Key,
                        kv.Value.Name
                    );
                    continue;
                }

                bool hasCurrentSleeve = currentTailToSleeveId.TryGetValue(kv.Key, out int currentSleeveId);

                if (!hasCurrentSleeve || currentSleeveId != kv.Value.Id)
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
    }
}
