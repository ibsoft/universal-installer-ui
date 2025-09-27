using System.Security.Principal;

namespace Installer.Core.Engine
{
    public sealed class AdminRightsCheck : IPreCheck
    {
        public string Name => "Admin Rights";
        public Task<Result> ValidateAsync(CancellationToken ct)
        {
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
            return Task.FromResult(isAdmin ? Result.Ok() : Result.Fail("Run as Administrator."));
        }
    }
}
