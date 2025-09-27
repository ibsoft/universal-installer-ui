namespace Installer.Core.Engine
{
    public sealed class DotnetDesktopRuntimeCheck : IPreCheck
    {
        private readonly Version _min;
        public DotnetDesktopRuntimeCheck(string min) { _min = new Version(min); }
        public string Name => ".NET Desktop Runtime";
        public Task<Result> ValidateAsync(CancellationToken ct)
        {
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "dotnet", "shared", "Microsoft.WindowsDesktop.App");
            if (!Directory.Exists(baseDir)) return Task.FromResult(Result.Fail(".NET Desktop Runtime not found."));
            var versions = Directory.GetDirectories(baseDir).Select(Path.GetFileName)!
                .Where(s=> Version.TryParse(s, out _))
                .Select(s=> new Version(s!));
            var ok = versions.Any(v => v >= _min);
            return Task.FromResult(ok ? Result.Ok() : Result.Fail($"Requires .NET Desktop Runtime {_min}+"));
        }
    }
}
