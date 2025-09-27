namespace Installer.Core.Util
{
    public static class EnvHelper
    {
        public static void SetMachine(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Machine);
        }
    }
}
