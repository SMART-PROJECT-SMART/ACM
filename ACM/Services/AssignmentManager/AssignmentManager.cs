using System.Collections.Concurrent;
using ACM.Models;
using ACM.Models.Dto;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.DeviceManagerClient.Interfaces;
using ACM.Services.SimulatorClient.Interfaces;
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
            ILogger<AssignmentManager> logger)
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
            CancellationToken cancellationToken = default)
        {
            List<ChangedAssignmentDto> changedAssignments = CollectChangedAssignments(newAssignment);
            HashSet<int> removedTails = _tailIdToSleeve.Keys.Except(newAssignment.Keys).ToHashSet();

            _logger.LogInformation(
                "SetAssignment: {NewCount} UAVs in new assignment, {ChangedCount} changed, {RemovedCount} removed",
                newAssignment.Count,
                changedAssignments.Count,
                removedTails.Count);

            if (changedAssignments.Count == 0 && removedTails.Count == 0)
            {
                _logger.LogInformation("No assignment changes; nothing to notify");
            }

            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                IDeviceManagerClient deviceManagerClient = scope.ServiceProvider.GetRequiredService<IDeviceManagerClient>();
                await NotifyDeviceManagerAsync(deviceManagerClient, changedAssignments, removedTails, cancellationToken);
            }

            UpdateInMemoryAssignment(newAssignment, removedTails);

            foreach (ChangedAssignmentDto change in changedAssignments)
            {
                try
                {
                    await _simulatorClient.NotifyUavPortsChangedAsync(change.TailId, change.NewPorts, cancellationToken);
                    _logger.LogInformation(
                        "Notified simulator of port change for UAV tail {TailId}, sleeve {SleeveName}",
                        change.TailId,
                        change.SleeveName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to notify simulator of port change for UAV tail {TailId}, sleeve {SleeveName}",
                        change.TailId,
                        change.SleeveName);
                }
            }
        }

        private List<ChangedAssignmentDto> CollectChangedAssignments(IReadOnlyDictionary<int, Sleeve> newAssignment)
        {
            List<ChangedAssignmentDto> result = new();
            foreach (KeyValuePair<int, Sleeve> kv in newAssignment)
            {
                if (!_tailIdToSleeve.TryGetValue(kv.Key, out Sleeve? currentSleeve) ||
                    currentSleeve.Name != kv.Value.Name)
                {
                    result.Add(new ChangedAssignmentDto
                    {
                        TailId = kv.Key,
                        SleeveName = kv.Value.Name,
                        NewPorts = kv.Value.PortNumbers
                    });
                }
            }

            return result;
        }

        private async Task NotifyDeviceManagerAsync(
            IDeviceManagerClient deviceManagerClient,
            List<ChangedAssignmentDto> changedAssignments,
            HashSet<int> removedTails,
            CancellationToken cancellationToken)
        {
            foreach (ChangedAssignmentDto change in changedAssignments)
            {
                try
                {
                    await deviceManagerClient.AssignSleeveToUavAsync(change.TailId, change.SleeveName, cancellationToken);
                    _logger.LogInformation(
                        "Changed sleeve for UAV tail {TailId} to sleeve {SleeveName}",
                        change.TailId,
                        change.SleeveName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to assign sleeve {SleeveName} to tail {TailId}", change.SleeveName, change.TailId);
                }
            }

            foreach (int tailId in removedTails)
            {
                try
                {
                    await deviceManagerClient.ReleaseSleeveByTailIdAsync(tailId, cancellationToken);
                    _logger.LogInformation("Released sleeve for UAV tail {TailId}", tailId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to release sleeve for tail {TailId}", tailId);
                }
            }
        }

        private void UpdateInMemoryAssignment(IReadOnlyDictionary<int, Sleeve> newAssignment, HashSet<int> removedTails)
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
