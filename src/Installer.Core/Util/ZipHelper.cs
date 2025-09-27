using System.IO.Compression;

namespace Installer.Core.Util
{
    public static class ZipHelper
    {
        public static void ExtractToDirectory(string zipPath, string destination, bool overwrite)
        {
            Directory.CreateDirectory(destination);
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
            {
                var fullPath = Path.Combine(destination, entry.FullName);
                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(fullPath);
                    continue;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                entry.ExtractToFile(fullPath, overwrite);
            }
        }
    }
}
