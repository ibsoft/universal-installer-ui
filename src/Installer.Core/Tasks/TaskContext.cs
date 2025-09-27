using Installer.Model.Config;
using Installer.Core.Util;
using Installer.Core.Engine;

namespace Installer.Core.Tasks
{
    public sealed class TaskContext
    {
        public InstallerConfig Config { get; }
        public FileLogger Logger { get; }
        public RollbackStack Rollback { get; }
        public string ReportDir { get; }

        public TaskContext(InstallerConfig cfg, FileLogger logger, RollbackStack rollback, string reportDir)
        {
            Config = cfg; Logger = logger; Rollback = rollback; ReportDir = reportDir;
        }

        public void Write(string msg) => Logger.Info(msg);
        public void PushRollback(Func<Task> action) => Rollback.Push(action);
    }
}
