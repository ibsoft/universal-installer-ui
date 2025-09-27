using System.Security.Cryptography;

namespace Installer.Core.Util
{
    public static class ChecksumVerifier
    {
        public static async Task<bool> VerifyAsync(string file, string expectedSha256)
        {
            if (string.IsNullOrWhiteSpace(expectedSha256)) return true;
            using var fs = File.OpenRead(file);
            using var sha = SHA256.Create();
            var hash = await sha.ComputeHashAsync(fs);
            var actual = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return string.Equals(actual, expectedSha256.ToLowerInvariant());
        }
    }
}
