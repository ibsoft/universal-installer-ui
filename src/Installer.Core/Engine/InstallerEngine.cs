using Installer.Core.Tasks;
using Installer.Core.Util;
using Installer.Model.Config;
using InstallerTaskFactory = Installer.Core.Tasks.TaskFactory;

namespace Installer.Core.Engine
{
    public enum Phase { Detect, PreChecks, Install, Post, Uninstall, Done }

    public sealed class InstallerEngine
    {
        private readonly InstallerConfig _cfg;
        private readonly FileLogger _log;
        private readonly TaskRunner _runner = new();
        private readonly RollbackStack _rollback = new();
        private readonly TelemetryClient _telemetry;

        public event EventHandler<string>? Log;
        public event EventHandler<int>? ProgressChanged { add => _runner.ProgressChanged += value; remove => _runner.ProgressChanged -= value; }
        public event EventHandler<int>? TaskProgressChanged { add => _runner.TaskProgressChanged += value; remove => _runner.TaskProgressChanged -= value; }
        public event EventHandler<string>? StatusChanged { add => _runner.StatusChanged += value; remove => _runner.StatusChanged -= value; }
        public event EventHandler<string>? TaskStarted { add => _runner.TaskStarted += value; remove => _runner.TaskStarted -= value; }
        public event EventHandler<string>? TaskCompleted { add => _runner.TaskCompleted += value; remove => _runner.TaskCompleted -= value; }
        public event EventHandler<Phase>? PhaseChanged;

        public InstallerEngine(InstallerConfig cfg, FileLogger logger)
        {
            _cfg = cfg; _log = logger;
            _telemetry = new TelemetryClient(cfg.Telemetry?.Enabled == true ? cfg.Telemetry?.Endpoint : null);
        }

        private void Write(string msg){ _log.Info(msg); Log?.Invoke(this, msg); }

        public async Task<Result> RunInstallAsync(CancellationToken ct)
        {
            _telemetry.TrySend("start", new{ mode="install", app=_cfg.Meta?.Name, version=_cfg.Meta?.Version });
            try
            {
                PhaseChanged?.Invoke(this, Phase.PreChecks);
                Write("Running pre-checks…");
                var checks = PreCheckFactory.Create(_cfg);
                foreach (var c in checks)
                {
                    var res = await c.ValidateAsync(ct);
                    if (!res.Success) return Result.Fail(res.Error!);
                }

                var ctx = new TaskContext(_cfg, _log, _rollback, reportDir: GetReportDir());
                PhaseChanged?.Invoke(this, Phase.Install);
                var tasks = InstallerTaskFactory.CreateInstallTasks(_cfg);
                HookTaskLogging(tasks);
                var res2 = await _runner.RunAsync(tasks, ctx, ct);
                if (!res2.Success) return await FailWithRollbackAsync(res2.Error!);

                PhaseChanged?.Invoke(this, Phase.Post);
                var post = InstallerTaskFactory.CreatePostTasks(_cfg);
                HookTaskLogging(post);
                var postRes = await _runner.RunAsync(post, ctx, ct);
                if (!postRes.Success) return await FailWithRollbackAsync(postRes.Error!);

                PhaseChanged?.Invoke(this, Phase.Done);
                Write("Install completed.");
                _telemetry.TrySend("end", new{ success=true});
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return await FailWithRollbackAsync(ex.Message);
            }
        }

        public async Task<Result> RunUninstallAsync(CancellationToken ct)
        {
            _telemetry.TrySend("start", new{ mode="uninstall", app=_cfg.Meta?.Name, version=_cfg.Meta?.Version });
            try
            {
                var ctx = new TaskContext(_cfg, _log, _rollback, reportDir: GetReportDir());
                PhaseChanged?.Invoke(this, Phase.Uninstall);
                var pre = InstallerTaskFactory.CreateUninstallPreTasks(_cfg);
                HookTaskLogging(pre);
                var resPre = await _runner.RunAsync(pre, ctx, ct);
                if (!resPre.Success) return await FailWithRollbackAsync(resPre.Error!);

                var tasks = InstallerTaskFactory.CreateUninstallTasks(_cfg);
                HookTaskLogging(tasks);
                var res = await _runner.RunAsync(tasks, ctx, ct);
                if (!res.Success) return await FailWithRollbackAsync(res.Error!);

                Write("Uninstall completed.");
                _telemetry.TrySend("end", new{ success=true});
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return await FailWithRollbackAsync(ex.Message);
            }
        }

        public async Task<Result> RunRepairAsync(CancellationToken ct)
        {
            Write("Repair flow: re-run install tasks idempotently.");
            return await RunInstallAsync(ct);
        }

        private async Task<Result> FailWithRollbackAsync(string error)
        {
            Write($"Failure encountered: {error}. Rolling back…");
            await _rollback.ExecuteAsync();
            _telemetry.TrySend("end", new{ success=false, error});
            return Result.Fail(error);
        }

        private string GetReportDir()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "UniversalInstaller", "reports");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private void HookTaskLogging(IEnumerable<IInstallerTask> tasks)
        {
            foreach (var t in tasks)
            {
                t.Log += (s, m) => Write($"[{t.DisplayName}] {m}");
            }
        }
    }
}
