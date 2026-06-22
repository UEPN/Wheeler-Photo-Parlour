using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WheelerPhotoParlour.Services
{
    public static class PhotoService
    {
        private static readonly string CacheDir = Path.Combine(Path.GetTempPath(), "WheelerPhotoParlour", "Cache");
        private static readonly ConcurrentDictionary<string, BitmapImage> ThumbnailCache = new();

        public static void InitCache()
        {
            Directory.CreateDirectory(CacheDir);
        }

        public static void ClearCache()
        {
            ThumbnailCache.Clear();
        }

        public static string GetCachePath(string sourcePath)
        {
            return Path.Combine(CacheDir, Path.GetFileName(sourcePath) + ".jpg");
        }

        public static bool ConvertToJpg(string sourcePath, string cachePath)
        {
            try
            {
                var bytes = File.ReadAllBytes(sourcePath);
                if (bytes.Length <= 300) return false;

                var imageBytes = new byte[bytes.Length - 300];
                Array.Copy(bytes, 300, imageBytes, 0, imageBytes.Length);
                File.WriteAllBytes(cachePath, imageBytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<BitmapImage?> LoadThumbnailAsync(string imagePath, int width, int height)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            var key = $"{imagePath}_{width}x{height}";
            if (ThumbnailCache.TryGetValue(key, out var cached))
                return cached;

            try
            {
                var bitmap = await Task.Run(() =>
                {
                    var bmp = new BitmapImage();
                    using var stream = File.OpenRead(imagePath);
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.DecodePixelWidth = width;
                    bmp.DecodePixelHeight = height;
                    bmp.StreamSource = stream;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                });

                ThumbnailCache[key] = bitmap;
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<BitmapImage?> LoadFullImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            try
            {
                return await Task.Run(() =>
                {
                    var bmp = new BitmapImage();
                    using var stream = File.OpenRead(imagePath);
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = stream;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                });
            }
            catch
            {
                return null;
            }
        }
    }
}
