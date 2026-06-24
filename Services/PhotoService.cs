using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WheelerPhotoParlour.Services
{
    /// <summary>PRDR 文件元数据。</summary>
    public class PhotoMetaInfo
    {
        /// <summary>拍摄地点，解析失败为 null。</summary>
        public string? Title { get; set; }

        /// <summary>玩家备注，未写则为 null。</summary>
        public string? Description { get; set; }

        /// <summary>游戏内世界时间，非文件创建时间。</summary>
        public DateTime? GameDateTime { get; set; }

        /// <summary>真实世界拍摄时间，非文件创建时间。</summary>
        public DateTime? RealWorldDateTime { get; set; }
    }

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

        // ============ 元数据解析 ============
        //
        // PRDR 文件结构（已验证）：
        //   [文件头] → "JPEG" + 长度 + JPEG图片数据
        //   → "JSON" + 长度 + JSON（坐标/时间等）
        //   → "TITL" + 长度 + UTF-8 地点名
        //   → "DESC" + 长度 + UTF-8 备注
        //   → "JEND"（文件末尾）

        // 缓冲区容量上限（真实样本：TITL/DESC=256，JSON=3072）
        private const int MaxDeclaredBufferBytes = 8192;

        private static readonly byte[] TitleMarker = Encoding.ASCII.GetBytes("TITL");
        private static readonly byte[] DescMarker = Encoding.ASCII.GetBytes("DESC");
        private static readonly byte[] JsonMarker = Encoding.ASCII.GetBytes("JSON");
        private static readonly byte[] JpegMarker = Encoding.ASCII.GetBytes("JPEG");

        private static readonly Regex RealWorldDateTimeRegex = new(@"(\d{1,2})/(\d{1,2})/(\d{2,4})\s+(\d{1,2}):(\d{2}):(\d{2})", RegexOptions.Compiled);

        public static PhotoMetaInfo ExtractMeta(string sourcePath)
        {
            var fileName = Path.GetFileName(sourcePath);
            var result = new PhotoMetaInfo();

            try
            {
                var bytes = File.ReadAllBytes(sourcePath);
                if (bytes.Length < 16) return result;

                result.Title = TryDecodeMarkerText(bytes, TitleMarker, fileName, "TITL(地点)");
                result.Description = TryDecodeMarkerText(bytes, DescMarker, fileName, "DESC(备注)");
                result.GameDateTime = TryParseGameDateTime(bytes, fileName);
                result.RealWorldDateTime = TryParseRealWorldDateTime(bytes, fileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: 解析异常 {ex.Message}");
            }

            return result;
        }

        /// <summary>搜索真实世界拍摄时间。在JPEG标记前的文件头区域用UTF-16LE解码，正则匹配日期。</summary>
        private static DateTime? TryParseRealWorldDateTime(byte[] bytes, string fileName)
        {
            try
            {
                // 找 JPEG 标记，确定图片数据起始
                var jpegIndex = FindMarker(bytes, JpegMarker, 0, bytes.Length);
                if (jpegIndex < 0)
                {
                    Debug.WriteLine($"[PhotoMeta] {fileName}: 未找到 JPEG 标记，无法解析真实世界时间");
                    return null;
                }

                // 搜索范围：文件头到 JPEG 标记之前
                var searchEnd = Math.Min(jpegIndex, bytes.Length);

                // UTF-16LE 解码搜索区域
                var headerText = Encoding.Unicode.GetString(bytes, 0, searchEnd);

                var match = RealWorldDateTimeRegex.Match(headerText);
                if (!match.Success)
                {
                    Debug.WriteLine($"[PhotoMeta] {fileName}: 未在文件头中匹配到真实世界时间");
                    return null;
                }

                var month = int.Parse(match.Groups[1].Value);
                var day = int.Parse(match.Groups[2].Value);
                var year = int.Parse(match.Groups[3].Value);
                // 两位年份补全为四位
                if (year < 100) year += 2000;
                var hour = int.Parse(match.Groups[4].Value);
                var minute = int.Parse(match.Groups[5].Value);
                var second = int.Parse(match.Groups[6].Value);

                var realTime = new DateTime(year, month, day, hour, minute, second);
                Debug.WriteLine($"[PhotoMeta] {fileName}: 解析出真实世界时间 {realTime:yyyy-MM-dd HH:mm:ss}");
                return realTime;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: 真实世界时间解析失败 {ex.Message}");
                return null;
            }
        }

        /// <summary>查找4字节标记，按"标记+4字节长度+缓冲区"解码文本。TITL/DESC共用。</summary>
        private static string? TryDecodeMarkerText(byte[] bytes, byte[] marker, string fileName, string label)
        {
            var markerIndex = FindMarker(bytes, marker, 0, bytes.Length);
            if (markerIndex < 0)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: 未找到 {label} 标记");
                return null;
            }

            var afterMarker = markerIndex + marker.Length;
            if (afterMarker + 4 > bytes.Length)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: {label} 标记后字节不足，无法读取长度字段");
                return null;
            }

            var declaredLen = BitConverter.ToUInt32(bytes, afterMarker);
            var textStart = afterMarker + 4;

            if (declaredLen == 0 || declaredLen > MaxDeclaredBufferBytes || textStart + declaredLen > bytes.Length)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: {label} 缓冲区容量异常（{declaredLen}），跳过");
                return null;
            }

            var candidate = DecodeAndSanitizeText(bytes, textStart, (int)declaredLen);
            if (candidate != null)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: {label} 命中，offset={markerIndex}，缓冲区容量={declaredLen}，结果=\"{candidate}\"");
            }
            else
            {
                // 空缓冲区（没写备注）是正常情况
                Debug.WriteLine($"[PhotoMeta] {fileName}: {label} 缓冲区为空或不含有效文字（如果是 DESC 备注，这通常只是玩家没写而已）");
            }
            return candidate;
        }

        /// <summary>解析JSON标记后的游戏内世界时间（time字段）。</summary>
        private static DateTime? TryParseGameDateTime(byte[] bytes, string fileName)
        {
            var markerIndex = FindMarker(bytes, JsonMarker, 0, bytes.Length);
            if (markerIndex < 0)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: 未找到 JSON 标记，无法解析游戏内时间");
                return null;
            }

            var afterMarker = markerIndex + JsonMarker.Length;
            if (afterMarker + 4 > bytes.Length) return null;

            var declaredLen = BitConverter.ToUInt32(bytes, afterMarker);
            var textStart = afterMarker + 4;
            if (declaredLen == 0 || declaredLen > MaxDeclaredBufferBytes * 8 || textStart + declaredLen > bytes.Length)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: JSON 缓冲区容量异常（{declaredLen}），跳过");
                return null;
            }

            try
            {
                var nullIndex = Array.IndexOf<byte>(bytes, 0, textStart, (int)declaredLen);
                var actualLen = nullIndex >= 0 ? nullIndex - textStart : (int)declaredLen;
                if (actualLen <= 0) return null;

                using var doc = JsonDocument.Parse(bytes.AsMemory(textStart, actualLen));
                if (!doc.RootElement.TryGetProperty("time", out var timeElement))
                {
                    Debug.WriteLine($"[PhotoMeta] {fileName}: JSON 中没有 time 字段");
                    return null;
                }

                var year = timeElement.GetProperty("year").GetInt32();
                var month = timeElement.GetProperty("month").GetInt32();
                var day = timeElement.GetProperty("day").GetInt32();
                var hour = timeElement.GetProperty("hour").GetInt32();
                var minute = timeElement.GetProperty("minute").GetInt32();
                var second = timeElement.GetProperty("second").GetInt32();

                var gameTime = new DateTime(year, month, day, hour, minute, second);
                Debug.WriteLine($"[PhotoMeta] {fileName}: 解析出游戏内时间 {gameTime:yyyy-MM-dd HH:mm:ss}");
                return gameTime;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PhotoMeta] {fileName}: JSON/游戏内时间解析失败 {ex.Message}");
                return null;
            }
        }

        /// <summary>UTF-8解码字节并做合理性校验，不合法返回null。</summary>
        private static string? DecodeAndSanitizeText(byte[] bytes, int start, int length)
        {
            try
            {
                // 截断到第一个\0，避免填充字节混入
                var nullIndex = Array.IndexOf<byte>(bytes, 0, start, length);
                var actualLen = nullIndex >= 0 ? nullIndex - start : length;
                if (actualLen <= 0) return null; // 空缓冲区（没写备注等）属于正常情况

                var text = Encoding.UTF8.GetString(bytes, start, actualLen).Trim();
                if (string.IsNullOrWhiteSpace(text)) return null;

                // 合理性校验：至少要有一个"字母类"字符（覆盖中文/英文/大多数语言），
                // 否则很可能是解析偏移算错了，读到了一堆乱码或二进制噪音
                if (!text.Any(c => char.IsLetter(c))) return null;

                // 过滤掉控制字符（正常文本不会包含），出现的话基本可以判定解析跑偏了
                if (text.Any(c => char.IsControl(c))) return null;

                return text;
            }
            catch
            {
                return null;
            }
        }

        private static int FindMarker(byte[] haystack, byte[] marker, int start, int end)
        {
            var searchEnd = Math.Min(end, haystack.Length) - marker.Length;
            for (int i = Math.Max(0, start); i <= searchEnd; i++)
            {
                var matched = true;
                for (int j = 0; j < marker.Length; j++)
                {
                    if (haystack[i + j] != marker[j]) { matched = false; break; }
                }
                if (matched) return i;
            }
            return -1;
        }

        /// <summary>清洗字符串为安全文件名，去掉非法字符、收紧空格、限制长度。</summary>
        public static string SanitizeForFileSystem(string input, int maxLength = 40)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                sb.Append(invalidChars.Contains(c) ? '_' : c);
            }

            var cleaned = sb.ToString().Trim().TrimEnd('.', ' ');
            // 收紧连续空格
            while (cleaned.Contains("  ")) cleaned = cleaned.Replace("  ", " ");

            if (cleaned.Length > maxLength) cleaned = cleaned.Substring(0, maxLength).TrimEnd();
            return cleaned;
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
