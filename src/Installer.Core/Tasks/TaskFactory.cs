using Installer.Model.Config;

namespace Installer.Core.Tasks
{
    public static class TaskFactory
    {
        public static List<IInstallerTask> CreateInstallTasks(InstallerConfig cfg)
        {
            var list = new List<IInstallerTask>();
            foreach (var t in cfg.Tasks ?? Enumerable.Empty<TaskSpec>())
                list.Add(CreateFromSpec(t, cfg));
            return list;
        }
        public static List<IInstallerTask> CreatePostTasks(InstallerConfig cfg)
        {
            var list = new List<IInstallerTask>();
            foreach (var t in cfg.PostTasks ?? Enumerable.Empty<TaskSpec>())
                list.Add(CreateFromSpec(t, cfg));
            return list;
        }
        public static List<IInstallerTask> CreateUninstallPreTasks(InstallerConfig cfg)
        {
            var list = new List<IInstallerTask>();
            foreach (var t in cfg.Uninstall?.PreTasks ?? Enumerable.Empty<TaskSpec>())
                list.Add(CreateFromSpec(t, cfg));
            return list;
        }
        public static List<IInstallerTask> CreateUninstallTasks(InstallerConfig cfg)
        {
            var list = new List<IInstallerTask>();
            foreach (var t in cfg.Uninstall?.Tasks ?? Enumerable.Empty<TaskSpec>())
                list.Add(CreateFromSpec(t, cfg));
            return list;
        }

        private static IInstallerTask CreateFromSpec(TaskSpec t, InstallerConfig cfg)
        {
            return t.Type!.ToLowerInvariant() switch
            {
                "runartifact" => new RunArtifactTask(t.Ref!),
                "unzip" => new UnzipTask(t.Ref!, t.TargetDir),
                "copy" => new CopyTask(t.Source!, t.TargetDir!, t.Recursive),
                "serviceinstall" => new ServiceInstallTask(t.Name!, t.DisplayName ?? t.Name!, t.BinaryPath!, t.StartMode ?? "Automatic", t.Account ?? "LocalSystem"),
                "servicestart" => new ServiceStartTask(t.Name!),
                "servicestop" => new ServiceStopTask(t.Name!),
                "servicedelete" => new ServiceDeleteTask(t.Name!),
                "registryset" => new RegistrySetTask(t.Path!, t.Name!, t.Value!, t.Kind ?? "String"),
                "envset" => new EnvSetTask(t.EnvName ?? t.Name!, t.Value!),
                "shortcut" => new ShortcutTask(t.Target!, t.Location!, t.Name!, t.IconPath),
                "powershell" => new PowerShellTask(t.Script!, t.Args, t.TimeoutSec ?? 300, t.ContinueOnError),
                "msiuninstall" => new MsiUninstallTask(t.ProductCode!),
                "removedir" => new RemoveDirTask(t.Path!, t.Recursive),
                _ => throw new NotSupportedException($"Unknown task type: {t.Type}")
            };
        }
    }
}
