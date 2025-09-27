using System.Net.NetworkInformation;

namespace Installer.Core.Engine
{
    public sealed class PortsInUseCheck : IPreCheck
    {
        private readonly int[] _ports;
        public PortsInUseCheck(IEnumerable<int> ports){ _ports = ports.ToArray(); }
        public string Name => "Ports Availability";
        public Task<Result> ValidateAsync(CancellationToken ct)
        {
            var ip = IPGlobalProperties.GetIPGlobalProperties();
            var listeners = ip.GetActiveTcpListeners().Select(e=> e.Port).ToHashSet();
            var conflicts = _ports.Where(p=> listeners.Contains(p)).ToArray();
            if (conflicts.Length>0) return Task.FromResult(Result.Fail($"Ports in use: {string.Join(", ", conflicts)}"));
            return Task.FromResult(Result.Ok());
        }
    }
}
