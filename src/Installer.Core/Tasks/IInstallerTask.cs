using Installer.Core.Engine;

namespace Installer.Core.Tasks
{
    public interface IInstallerTask
    {
        string DisplayName { get; }
        event EventHandler<int>? Progress;
        event EventHandler<string>? Log;
        Task<Result> RunAsync(TaskContext ctx, CancellationToken ct);
    }
}
