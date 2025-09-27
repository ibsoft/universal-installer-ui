namespace Installer.Core.Engine
{
    public interface IPreCheck
    {
        string Name { get; }
        Task<Result> ValidateAsync(CancellationToken ct);
    }
}
