using Installer.Core.Engine;
using Installer.Core.Util;

namespace Installer.Core.Tasks
{
    public sealed class UnzipTask : IInstallerTask
    {
        public string DisplayName => "Unzip";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _refId;
        private readonly string? _targetDir;
        public UnzipTask(string refId, string? targetDir){ _refId = refId; _targetDir = targetDir; }
        public Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            var art = ctx.Config.Artifacts!.First(a=> a.Id == _refId);
            Progress?.Invoke(this, 0);
            if (!string.Equals(art.Type, "zip", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(Result.Fail("Artifact is not zip"));
            var src = Path.IsPathRooted(art.Source) ? art.Source : Path.Combine(AppContext.BaseDirectory, art.Source);
            var targetRaw = _targetDir ?? art.TargetDir ?? throw new InvalidOperationException("targetDir required");
            var target = PathUtil.Expand(targetRaw);
            ZipHelper.ExtractToDirectory(src, target, overwrite:true);
            Log?.Invoke(this, $"Extracted {src} -> {target}");
            Progress?.Invoke(this, 100);
            return Task.FromResult(Result.Ok());
        }
    }
}
