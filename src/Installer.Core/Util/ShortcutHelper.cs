namespace Installer.Core.Util
{
    public static class ShortcutHelper
    {
        public static void CreateShortcut(string targetPath, string location, string name, string? iconPath)
        {
            string baseDir = location.ToLowerInvariant() switch
            {
                "desktop" => Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                "startmenu" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
                _ => Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
            };
            Directory.CreateDirectory(baseDir);
            string lnk = Path.Combine(baseDir, name + ".lnk");

            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) throw new InvalidOperationException("WScript.Shell COM not available");
            dynamic shell = Activator.CreateInstance(shellType)!;
            var shortcut = shell.CreateShortcut(lnk);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(iconPath)) shortcut.IconLocation = iconPath;
            shortcut.Save();
        }
    }
}
