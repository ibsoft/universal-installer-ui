using Installer.Model.Config;

namespace Installer.Core.Engine
{
    public static class PreCheckFactory
    {
        public static IEnumerable<IPreCheck> Create(InstallerConfig cfg)
        {
            foreach (var c in cfg.PreChecks ?? Enumerable.Empty<PreCheckSpec>())
            {
                switch (c.Type?.ToLowerInvariant())
                {
                    case "adminrights":
                        yield return new AdminRightsCheck();
                        break;
                    case "osversion":
                        if (!string.IsNullOrWhiteSpace(c.Min))
                            yield return new OsVersionCheck(c.Min!);
                        break;
                    case "dotnetdesktopruntime":
                        if (!string.IsNullOrWhiteSpace(c.Min))
                            yield return new DotnetDesktopRuntimeCheck(c.Min!);
                        break;
                    case "diskspace":
                        yield return new DiskSpaceCheck(c.Drive ?? "C:", c.MinMB ?? 1024);
                        break;
                    case "ports":
                        if (c.Ports != null && c.Ports.Length > 0) yield return new PortsInUseCheck(c.Ports);
                        break;
                }
            }
        }
    }
}
