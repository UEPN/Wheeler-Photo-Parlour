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

        private bool _isSelected;
        /// <summary>是否被用户勾选，用于"导出勾选项"功能</summary>
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
