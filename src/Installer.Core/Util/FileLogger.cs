namespace Installer.Core.Util
{
    public sealed class FileLogger
    {
        private readonly string _file;
        private readonly int _maxSizeBytes;
        private readonly int _maxFiles;
        private readonly object _lock = new();

        public FileLogger(string file, int maxSizeMB, int maxFiles)
        {
            _file = Environment.ExpandEnvironmentVariables(file);
            _maxSizeBytes = Math.Max(1, maxSizeMB) * 1024 * 1024;
            _maxFiles = Math.Max(1, maxFiles);
            Directory.CreateDirectory(Path.GetDirectoryName(_file)!);
        }

        public void Info(string message) => Write("INFO", message);
        public void Error(string message) => Write("ERROR", message);

        private void Write(string level, string message)
        {
            lock(_lock)
            {
                RotateIfNeeded();
                File.AppendAllText(_file, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level} {message}{Environment.NewLine}");
            }
        }

        private void RotateIfNeeded()
        {
            try
            {
                if (File.Exists(_file) && new FileInfo(_file).Length > _maxSizeBytes)
                {
                    for (int i=_maxFiles-1; i>=1; i--)
                    {
                        var src = $"{_file}.{i}";
                        var dst = $"{_file}.{i+1}";
                        if (File.Exists(dst)) File.Delete(dst);
                        if (File.Exists(src)) File.Move(src, dst);
                    }
                    var first = _file + ".1";
                    if (File.Exists(first)) File.Delete(first);
                    File.Move(_file, first);
                }
            }
            catch { }
        }
    }
}
