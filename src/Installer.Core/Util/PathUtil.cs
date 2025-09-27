namespace Installer.Core.Util
{
    public static class PathUtil
    {
        public static string Expand(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path ?? string.Empty;
            var expanded = Environment.ExpandEnvironmentVariables(path);
            try
            {
                if (!System.IO.Path.IsPathFullyQualified(expanded))
                {
                    expanded = System.IO.Path.GetFullPath(expanded);
                }
            }
            catch { }
            return expanded;
        }
    }
}
