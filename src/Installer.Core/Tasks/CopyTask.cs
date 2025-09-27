using Installer.Core.Engine;
using Installer.Core.Util;

namespace Installer.Core.Tasks
{
    public sealed class CopyTask : IInstallerTask
    {
        public string DisplayName => "Copy Files";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _source;
        private readonly string _target;
        private readonly bool _recursive;
        public CopyTask(string source, string target, bool recursive){ _source=source; _target=target; _recursive=recursive; }
        public Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            var srcRaw = PathUtil.Expand(_source);
            var src = Path.IsPathRooted(srcRaw)? srcRaw : Path.Combine(AppContext.BaseDirectory, srcRaw);
            var dst = PathUtil.Expand(_target);
            Directory.CreateDirectory(dst);
            if (File.Exists(src))
            {
                File.Copy(src, Path.Combine(dst, Path.GetFileName(src)), overwrite:true);
            }
            else if (Directory.Exists(src))
            {
                foreach(var file in Directory.EnumerateFiles(src, "*", _recursive?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly))
                {
                    var rel = Path.GetRelativePath(src, file);
                    var destPath = Path.Combine(dst, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                    File.Copy(file, destPath, overwrite:true);
                }
            }
            else return Task.FromResult(Result.Fail($"Source not found: {src}"));
            Log?.Invoke(this, $"Copied from {src} to {dst}");
            return Task.FromResult(Result.Ok());
        }
    }
}
