using Installer.Core.Tasks;

namespace Installer.Core.Engine
{
    public sealed class TaskRunner
    {
        public event EventHandler<int>? ProgressChanged;
        public event EventHandler<int>? TaskProgressChanged;
        public event EventHandler<string>? TaskStarted;
        public event EventHandler<string>? TaskCompleted;
        public event EventHandler<string>? StatusChanged;

        public async Task<Result> RunAsync(IReadOnlyList<IInstallerTask> tasks, TaskContext ctx, CancellationToken ct)
        {
            for (int i=0; i<tasks.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var t = tasks[i];
                StatusChanged?.Invoke(this, $"Running: {t.DisplayName}");
                TaskStarted?.Invoke(this, t.DisplayName);
                TaskProgressChanged?.Invoke(this, 0);
                t.Progress += (s, p) => TaskProgressChanged?.Invoke(this, p);
                var res = await t.RunAsync(ctx, ct);
                if (!res.Success) return res;
                TaskCompleted?.Invoke(this, t.DisplayName);
                ProgressChanged?.Invoke(this, (int)Math.Round(((i+1)/(double)tasks.Count)*100));
            }
            return Result.Ok();
        }
    }
}
