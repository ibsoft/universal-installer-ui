using Installer.Core.Engine;
using Installer.Core.Util;

namespace Installer.Core.Tasks
{
    public sealed class RegistrySetTask : IInstallerTask
    {
        public string DisplayName => "Registry Set";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _path,_name,_value,_kind;
        public RegistrySetTask(string path,string name,string value,string kind){_path=path;_name=name;_value=value;_kind=kind;}
        public Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            Progress?.Invoke(this, 0);
            RegistryHelper.SetValue(_path, _name, _value, _kind);
            Log?.Invoke(this, $"Set {_path}::{_name}");
            Progress?.Invoke(this, 100);
            return Task.FromResult(Result.Ok());
        }
    }

    public sealed class EnvSetTask : IInstallerTask
    {
        public string DisplayName => "Env Var Set";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _name,_value;
        public EnvSetTask(string name,string value){_name=name;_value=value;}
        public Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            EnvHelper.SetMachine(_name, _value);
            Log?.Invoke(this, $"Set env {_name}");
            return Task.FromResult(Result.Ok());
        }
    }

    public sealed class ShortcutTask : IInstallerTask
    {
        public string DisplayName => "Create Shortcut";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _target,_location,_name,_icon;
        public ShortcutTask(string target,string location,string name,string? iconPath){_target=target;_location=location;_name=name;_icon=iconPath??string.Empty;}
        public Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            ShortcutHelper.CreateShortcut(_target, _location, _name, string.IsNullOrWhiteSpace(_icon)? null: _icon);
            Log?.Invoke(this, $"Shortcut '{_name}' created");
            return Task.FromResult(Result.Ok());
        }
    }

    public sealed class PowerShellTask : IInstallerTask
    {
        public string DisplayName => "PowerShell Script";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _script,_args; private readonly int _timeout; private readonly bool _continue;
        public PowerShellTask(string script,string? args,int timeoutSec,bool continueOnError){_script=script;_args=args??string.Empty;_timeout=timeoutSec;_continue=continueOnError;}
        public async Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            var path = Path.IsPathRooted(_script)? _script : Path.Combine(AppContext.BaseDirectory, _script);
            if (!File.Exists(path)) return Result.Fail($"Script not found: {path}");
            var cmd = $"-NoProfile -ExecutionPolicy Bypass -File \"{path}\" {_args}";
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_timeout));
            var (code, _, err) = await ProcessRunner.RunAsync("powershell.exe", cmd, cts.Token, p=>Progress?.Invoke(this,p));
            if (code!=0 && !_continue) return Result.Fail($"PowerShell failed: {err}");
            return Result.Ok();
        }
    }

    public sealed class MsiUninstallTask : IInstallerTask
    {
        public string DisplayName => "MSI Uninstall";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _productCode;
        public MsiUninstallTask(string productCode){ _productCode = productCode; }
        public async Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            var args = $"/x {_productCode} /qn /norestart";
            var (code, _, err) = await ProcessRunner.RunAsync("msiexec.exe", args, ct, p=>Progress?.Invoke(this,p));
            if (code!=0) return Result.Fail($"MSI uninstall failed: {err}");
            return Result.Ok();
        }
    }

    public sealed class RemoveDirTask : IInstallerTask
    {
        public string DisplayName => "Remove Directory";
        public event EventHandler<int>? Progress;
        public event EventHandler<string>? Log;
        private readonly string _path; private readonly bool _recursive;
        public RemoveDirTask(string path,bool recursive){_path=path;_recursive=recursive;}
        public Task<Result> RunAsync(TaskContext ctx, CancellationToken ct)
        {
            var p = PathUtil.Expand(_path);
            Progress?.Invoke(this, 0);
            if (Directory.Exists(p)) Directory.Delete(p, recursive:_recursive);
            Progress?.Invoke(this, 100);
            return Task.FromResult(Result.Ok());
        }
    }
}
