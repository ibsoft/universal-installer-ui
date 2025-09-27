using Installer.Core.Engine;
using Installer.Core.Util;
using Installer.Model.Config;

namespace Installer.Core.Tasks
{
    public sealed class RunArtifactTask : IInstallerTask
    {
        public string DisplayName => "Run Artifact";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _refId;
        public RunArtifactTask(string refId){ _refId = refId; }

        public async Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            var art = ctx.Config.Artifacts!.First(a=> a.Id == _refId);
            Log?.Invoke(this, $"Preparing artifact {_refId} ({art.Type})");
            string localPath = await EnsureLocalAsync(art, ctx, ct);
            if (!string.IsNullOrEmpty(art.Sha256))
            {
                var ok = await ChecksumVerifier.VerifyAsync(localPath, art.Sha256);
                if (!ok) return Result.Fail($"Checksum mismatch for {_refId}");
            }

            switch (art.Type.ToLowerInvariant())
            {
                case "msi":
                    return await RunProcessAsync("msiexec.exe", $"/i \"{localPath}\" {art.SilentArgs ?? "/qn /norestart"}", ctx, ct);
                case "exe":
                    return await RunProcessAsync(localPath, art.SilentArgs ?? "/S", ctx, ct);
                case "msix":
                    return await RunProcessAsync("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -Command \"Add-AppxPackage -ForceApplicationShutdown -Path '{localPath}'\"", ctx, ct);
                case "zip":
                    if (string.IsNullOrWhiteSpace(art.TargetDir)) return Result.Fail("zip artifact requires targetDir");
                    ZipHelper.ExtractToDirectory(localPath, PathUtil.Expand(art.TargetDir!), overwrite:true);
                    Log?.Invoke(this, $"Extracted to {art.TargetDir}");
                    return Result.Ok();
                case "copy":
                    if (string.IsNullOrWhiteSpace(art.TargetDir)) return Result.Fail("copy artifact requires targetDir");
                    var tgt = PathUtil.Expand(art.TargetDir!);
                    Directory.CreateDirectory(tgt);
                    File.Copy(localPath, Path.Combine(tgt, Path.GetFileName(localPath)), overwrite:true);
                    return Result.Ok();
                default:
                    return Result.Fail($"Unsupported artifact type: {art.Type}");
            }
        }

        private async Task<string> EnsureLocalAsync(ArtifactSpec art, TaskContext ctx, CancellationToken ct)
        {
            if (Uri.IsWellFormedUriString(art.Source, UriKind.Absolute))
            {
                var temp = Path.Combine(Path.GetTempPath(), "UniversalInstaller", Path.GetFileName(new Uri(art.Source).LocalPath));
                Directory.CreateDirectory(Path.GetDirectoryName(temp)!);
                await Downloader.DownloadFileAsync(art.Source, temp, p=> Progress?.Invoke(this,p), ct);
                return temp;
            }
            else
            {
                var srcExpanded = PathUtil.Expand(art.Source);
                var p = Path.IsPathRooted(srcExpanded) ? srcExpanded : Path.Combine(AppContext.BaseDirectory, srcExpanded);
                return p;
            }
        }

        private async Task<Result> RunProcessAsync(string file, string args, TaskContext ctx, CancellationToken ct)
        {
            Log?.Invoke(this, $"Start: {file} {args}");
            var (code, stdout, stderr) = await ProcessRunner.RunAsync(file, args, ct, p=> Progress?.Invoke(this,p));
            if (code!=0) return Result.Fail($"Process failed ({code}): {stderr}");
            Log?.Invoke(this, stdout);
            return Result.Ok();
        }
    }
}
