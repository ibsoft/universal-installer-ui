using Microsoft.Win32;

namespace Installer.Core.Util
{
    public static class RegistryHelper
    {
        public static void SetValue(string path, string name, string value, string kind)
        {
            var split = SplitRoot(path);
            var root = split.root;
            var sub = split.subkey;

            using var key = root.CreateSubKey(sub, true)!;
            var regKind = (kind ?? "String").ToLowerInvariant() switch
            {
                "string" => RegistryValueKind.String,
                "dword" => RegistryValueKind.DWord,
                "qword" => RegistryValueKind.QWord,
                "expandstring" => RegistryValueKind.ExpandString,
                "multistring" => RegistryValueKind.MultiString,
                "binary" => RegistryValueKind.Binary,
                _ => RegistryValueKind.String
            };
            key.SetValue(name, value, regKind);
        }

        private static (RegistryKey root, string subkey) SplitRoot(string path)
        {
            path ??= string.Empty;

            if (path.StartsWith("HKLM", System.StringComparison.OrdinalIgnoreCase))
                return (Registry.LocalMachine, TrimSub(path, 4));
            if (path.StartsWith("HKEY_LOCAL_MACHINE", System.StringComparison.OrdinalIgnoreCase))
                return (Registry.LocalMachine, TrimSub(path, "HKEY_LOCAL_MACHINE".Length));

            if (path.StartsWith("HKCU", System.StringComparison.OrdinalIgnoreCase))
                return (Registry.CurrentUser, TrimSub(path, 4));
            if (path.StartsWith("HKEY_CURRENT_USER", System.StringComparison.OrdinalIgnoreCase))
                return (Registry.CurrentUser, TrimSub(path, "HKEY_CURRENT_USER".Length));

            if (path.StartsWith("HKCR", System.StringComparison.OrdinalIgnoreCase))
                return (Registry.ClassesRoot, TrimSub(path, 4));
            if (path.StartsWith("HKEY_CLASSES_ROOT", System.StringComparison.OrdinalIgnoreCase))
                return (Registry.ClassesRoot, TrimSub(path, "HKEY_CLASSES_ROOT".Length));

            if (path.StartsWith("HKU", System.StringComparison.OrdinalIgnoreCase))
                return (Registry.Users, TrimSub(path, 3));
            if (path.StartsWith("HKEY_USERS", System.StringComparison.OrdinalIgnoreCase))
                return (Registry.Users, TrimSub(path, "HKEY_USERS".Length));

            return (Registry.LocalMachine, TrimSub(path, 0));
        }

        private static string TrimSub(string text, int prefixLen)
        {
            string sub = text.Length > prefixLen ? text.Substring(prefixLen) : string.Empty;
            while (sub.StartsWith("\\")) sub = sub.Substring(1);
            return sub;
        }
    }
}
