using System.Diagnostics;

namespace Installer.Core.Util
{
    public static class ProcessRunner
    {
        public static async Task<(int exitCode, string stdout, string stderr)> RunAsync(string fileName, string args, CancellationToken ct, Action<int>? onProgress = null)
        {
            var psi = new ProcessStartInfo(fileName, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var stdout = new List<string>();
            var stderr = new List<string>();
            var tcs = new TaskCompletionSource<int>();
            p.OutputDataReceived += (s,e)=> { if (e.Data!=null) stdout.Add(e.Data); };
            p.ErrorDataReceived += (s,e)=> { if (e.Data!=null) stderr.Add(e.Data); };
            p.Exited += (s,e)=> tcs.TrySetResult(p.ExitCode);
            p.Start();
            p.BeginOutputReadLine(); p.BeginErrorReadLine();
            if (onProgress!=null) _ = Task.Run(async ()=> { int prog=0; while (!p.HasExited){ try{ await Task.Delay(1000, ct); } catch{} prog = Math.Min(99, prog+1); onProgress(prog);} onProgress(100); });
            using (ct.Register(()=> { try { if (!p.HasExited) p.Kill(true); } catch {} }))
            {
                var code = await tcs.Task;
                return (code, string.Join(Environment.NewLine, stdout), string.Join(Environment.NewLine, stderr));
            }
        }
    }
}
