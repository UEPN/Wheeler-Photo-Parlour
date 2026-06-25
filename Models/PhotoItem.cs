using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WheelerPhotoParlour.Services;

namespace WheelerPhotoParlour.Models
{
    public class PhotoItem : INotifyPropertyChanged
    {
        public string SourcePath { get; set; } = "";
        public string CachePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }

        /// <summary>拍摄地点，解析失败为 null。</summary>
        public string? LocationTitle { get; set; }

        /// <summary>游戏内世界时间，解析失败为 null。</summary>
        public DateTime? GameDateTime { get; set; }

        /// <summary>真实世界拍摄时间，解析失败为 null。</summary>
        public DateTime? RealWorldDateTime { get; set; }

        /// <summary>文件创建时间，命名兜底用。</summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>生成分类文件夹名，解析失败回退到占位名。</summary>
        public string GetLocationFolderName(string fallbackLabel)
        {
            var sanitized = PhotoService.SanitizeForFileSystem(LocationTitle ?? "");
            return string.IsNullOrEmpty(sanitized) ? fallbackLabel : sanitized;
        }

        /// <summary>生成导出基础文件名（"地点_时间"，不含扩展名）。根据timestampMode选时间来源。</summary>
        public string GetExportBaseName(string fallbackLabel, string timestampMode = "GameTime")
        {
            var location = GetLocationFolderName(fallbackLabel);
            DateTime timestamp;
            if (timestampMode == "RealTime")
            {
                timestamp = RealWorldDateTime ?? (CreatedTime == default ? DateTime.Now : CreatedTime);
            }
            else
            {
                timestamp = GameDateTime ?? (CreatedTime == default ? DateTime.Now : CreatedTime);
            }
            return $"{location}_{timestamp:yyyy-MM-dd_HH-mm-ss}";
        }

        private bool _isSelected;
        /// <summary>是否被勾选。</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage? _thumbnail;
        public BitmapImage? Thumbnail
        {
            get => _thumbnail;
            set
            {
                _thumbnail = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadThumbnailAsync()
        {
            Thumbnail = await PhotoService.LoadThumbnailAsync(CachePath, 240, 135);
        }

        public string FileSizeText
        {
            get
            {
                if (FileSize < 1024) return $"{FileSize} B";
                if (FileSize < 1024 * 1024) return $"{FileSize / 1024:N0} KB";
                return $"{FileSize / 1024 / 1024:N1} MB";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
