using System.Text.Json;

namespace Installer.Model.Config
{
    public static class JsonConfigLoader
    {
        public static InstallerConfig Load(string path, out string rawJson)
        {
            path = Path.IsPathRooted(path)? path: Path.Combine(AppContext.BaseDirectory, path);
            rawJson = File.ReadAllText(path);
            var cfg = JsonSerializer.Deserialize<InstallerConfig>(rawJson, new JsonSerializerOptions{PropertyNameCaseInsensitive=true}) ?? new InstallerConfig();
            return cfg;
        }
    }
}
