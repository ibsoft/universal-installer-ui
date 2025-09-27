using Installer.Core.Engine;
using Installer.Core.Util;
using System.ServiceProcess;

namespace Installer.Core.Tasks
{
    public sealed class ServiceInstallTask : IInstallerTask
    {
        public string DisplayName => "Service Install";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _name,_display,_bin,_start,_account;
        public ServiceInstallTask(string name,string display,string bin,string start,string account){_name=name;_display=display;_bin=bin;_start=start;_account=account;}
        public async Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            if (ServiceManager.Exists(_name)) { Log?.Invoke(this, "Service already exists (idempotent)"); return Result.Ok(); }
            Directory.CreateDirectory(Path.GetDirectoryName(_bin)!);
            var startType = _start.Equals("Automatic", StringComparison.OrdinalIgnoreCase)? "auto": _start.Equals("Manual", StringComparison.OrdinalIgnoreCase)? "demand":"disabled";
            var args = $"create \"{_name}\" binPath= \"{_bin}\" start= {startType} DisplayName= \"{_display}\"";
            var (code, _, err) = await ProcessRunner.RunAsync("sc.exe", args, ct);
            if (code!=0) return Result.Fail($"sc create failed: {err}");
            Log?.Invoke(this, "Service created");
            ctx.PushRollback(async ()=> await ProcessRunner.RunAsync("sc.exe", $"delete \"{_name}\"", CancellationToken.None));
            return Result.Ok();
        }
    }

    public sealed class ServiceStartTask : IInstallerTask
    {
        public string DisplayName => "Service Start";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _name; public ServiceStartTask(string name){ _name=name; }
        public async Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            if (!ServiceManager.Exists(_name)) return Result.Fail($"Service not found: {_name}");
            if (ServiceManager.GetStatus(_name) == ServiceControllerStatus.Running){ Log?.Invoke(this, "Already running"); return Result.Ok(); }
            var (code, _, err) = await ProcessRunner.RunAsync("sc.exe", $"start \"{_name}\"", ct);
            if (code!=0) return Result.Fail($"sc start failed: {err}");
            return Result.Ok();
        }
    }

    public sealed class ServiceStopTask : IInstallerTask
    {
        public string DisplayName => "Service Stop";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _name; public ServiceStopTask(string name){ _name=name; }
        public async Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            if (!ServiceManager.Exists(_name)) { Log?.Invoke(this, "Service missing"); return Result.Ok(); }
            var (code, _, err) = await ProcessRunner.RunAsync("sc.exe", $"stop \"{_name}\"", ct);
            if (code!=0) Log?.Invoke(this, $"sc stop returned {code}: {err}");
            return Result.Ok();
        }
    }

    public sealed class ServiceDeleteTask : IInstallerTask
    {
        public string DisplayName => "Service Delete";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _name; public ServiceDeleteTask(string name){ _name=name; }
        public async Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            if (!ServiceManager.Exists(_name)) return Result.Ok();
            var (code, _, err) = await ProcessRunner.RunAsync("sc.exe", $"delete \"{_name}\"", ct);
            if (code!=0) return Result.Fail($"sc delete failed: {err}");
            return Result.Ok();
        }
    }
}
