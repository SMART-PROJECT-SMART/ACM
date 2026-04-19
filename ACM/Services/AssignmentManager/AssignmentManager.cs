using ACM.Common;
using ACM.Models;
using ACM.Models.Dto;
using ACM.Services.AssignmentManager.Interfaces;
using ACM.Services.Clients.DeviceManagerClient.Interfaces;
using ACM.Services.Clients.SimulatorClient.Interfaces;
using Microsoft.Extensions.Logging;

namespace ACM.Services.AssignmentManager
{
    public class AssignmentManager : IAssignmentManager
    {
        private readonly IDeviceManagerClient _deviceManagerClient;
        private readonly ISimulatorClient _simulatorClient;
        private readonly ILogger<AssignmentManager> _logger;

        public AssignmentManager(
            IDeviceManagerClient deviceManagerClient,
            ISimulatorClient simulatorClient,
            ILogger<AssignmentManager> logger
        )
        {
            _deviceManagerClient = deviceManagerClient;
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

            await _deviceManagerClient.ApplyAssignmentChangesAsync(
                changedAssignments,
                new HashSet<int>(),
                cancellationToken
            );

            UavPortsChangedBatchRequestDto batchRequest = BuildBatchRequest(changedAssignments);
            await _simulatorClient.NotifyUavPortsChangedBatchAsync(batchRequest, cancellationToken);
            _logger.LogInformation(
                "Notified simulator of batched port change for {ChangedCount} UAVs",
                batchRequest.Changes.Count()
            );
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
                    throw new InvalidOperationException(
                        string.Format(ACMConstants.RemapErrorMessages.INVALID_SLEEVE_ID, kv.Key)
                    );
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

        private UavPortsChangedBatchRequestDto BuildBatchRequest(
            IEnumerable<ChangedAssignmentDto> changedAssignments
        )
        {
            HashSet<int> usedTargetPorts = new();
            List<UavPortsChangedRequestDto> changes = changedAssignments
                .Select(change => BuildAndValidateChange(change, usedTargetPorts))
                .ToList();
            return new UavPortsChangedBatchRequestDto { Changes = changes };
        }

        private UavPortsChangedRequestDto BuildAndValidateChange(
            ChangedAssignmentDto change,
            HashSet<int> usedTargetPorts
        )
        {
            List<int> ports = (change.NewPorts ?? []).ToList();
            ValidatePortCount(change.TailId, ports);
            ValidatePortUniqueness(change.TailId, ports);
            ValidatePortRange(change.TailId, ports);
            ValidateBatchPortUniqueness(ports, usedTargetPorts);
            return new UavPortsChangedRequestDto { TailId = change.TailId, NewPorts = ports };
        }

        private void ValidatePortCount(int tailId, IReadOnlyCollection<int> ports)
        {
            if (ports.Count != ACMConstants.Remap.EXPECTED_SLEEVE_PORT_COUNT)
            {
                throw new InvalidOperationException(
                    string.Format(ACMConstants.RemapErrorMessages.INVALID_PORT_PAIR_COUNT, tailId)
                );
            }
        }

        private void ValidatePortUniqueness(int tailId, IReadOnlyList<int> ports)
        {
            if (ports[0] == ports[1])
            {
                throw new InvalidOperationException(
                    string.Format(ACMConstants.RemapErrorMessages.DUPLICATE_PORT_IN_PAIR, tailId)
                );
            }
        }

        private void ValidatePortRange(int tailId, IEnumerable<int> ports)
        {
            foreach (int port in ports)
            {
                if (
                    port < ACMConstants.Remap.MIN_PORT_NUMBER
                    || port > ACMConstants.Remap.MAX_PORT_NUMBER
                )
                {
                    throw new InvalidOperationException(
                        string.Format(ACMConstants.RemapErrorMessages.PORT_OUT_OF_RANGE, tailId)
                    );
                }
            }
        }

        private void ValidateBatchPortUniqueness(
            IEnumerable<int> ports,
            HashSet<int> usedTargetPorts
        )
        {
            foreach (int port in ports)
            {
                if (!usedTargetPorts.Add(port))
                {
                    throw new InvalidOperationException(
                        string.Format(ACMConstants.RemapErrorMessages.DUPLICATE_TARGET_PORT, port)
                    );
                }
            }
        }
    }
}
