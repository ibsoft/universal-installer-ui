using System.ServiceProcess;

namespace Installer.Core.Util
{
    public static class ServiceManager
    {
        public static bool Exists(string serviceName)
        {
            try { return ServiceController.GetServices().Any(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase)); }
            catch { return false; }
        }

        public static ServiceControllerStatus GetStatus(string serviceName)
        {
            using var sc = new ServiceController(serviceName);
            return sc.Status;
        }
    }
}
