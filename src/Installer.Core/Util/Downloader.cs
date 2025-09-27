namespace Installer.Core.Util
{
    public static class Downloader
    {
        public static async Task DownloadFileAsync(string url, string destination, Action<int>? onProgress, CancellationToken ct)
        {
            using var http = new HttpClient();
            using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();
            var total = resp.Content.Headers.ContentLength ?? -1L;
            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            using var fs = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[81920];
            long read = 0;
            int r;
            while ((r = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await fs.WriteAsync(buffer, 0, r, ct);
                read += r;
                if (total > 0 && onProgress != null)
                {
                    onProgress((int)(read * 100 / total));
                }
            }
        }
    }
}
