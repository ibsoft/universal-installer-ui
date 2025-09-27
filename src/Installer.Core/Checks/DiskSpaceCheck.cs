namespace Installer.Core.Engine
{
    public sealed class DiskSpaceCheck : IPreCheck
    {
        private readonly string _drive;
        private readonly long _minMB;
        public DiskSpaceCheck(string drive, long minMB){ _drive = drive; _minMB = minMB; }
        public string Name => "Disk Space";
        public Task<Result> ValidateAsync(CancellationToken ct)
        {
            var di = new DriveInfo(_drive);
            var freeMB = di.AvailableFreeSpace / (1024*1024);
            return Task.FromResult(freeMB >= _minMB ? Result.Ok() : Result.Fail($"Need {_minMB}MB free on {_drive}."));
        }
    }
}
