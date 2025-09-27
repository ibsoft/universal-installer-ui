namespace Installer.Core.Engine
{
    public sealed class OsVersionCheck : IPreCheck
    {
        private readonly Version _min;
        public OsVersionCheck(string min) { _min = new Version(min); }
        public string Name => "OS Version";
        public Task<Result> ValidateAsync(CancellationToken ct)
        {
            var v = Environment.OSVersion.Version;
            return Task.FromResult(v >= _min ? Result.Ok() : Result.Fail($"Requires Windows {_min} or later."));
        }
    }
}
