using System.Text;
using System.Text.Json;

namespace Installer.Core.Util
{
    public sealed class TelemetryClient
    {
        private readonly string? _endpoint;
        public TelemetryClient(string? endpoint){ _endpoint = endpoint; }
        public void TrySend(string evt, object payload)
        {
            if (string.IsNullOrWhiteSpace(_endpoint)) return;
            try
            {
                using var http = new HttpClient();
                var obj = new { eventName = evt, data = payload, ts = DateTime.UtcNow };
                var json = JsonSerializer.Serialize(obj);
                var resp = http.PostAsync(_endpoint, new StringContent(json, Encoding.UTF8, "application/json")).Result;
            }
            catch { }
        }
    }
}
