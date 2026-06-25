using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic.FileIO;
using WheelerPhotoParlour.Models;
using WheelerPhotoParlour.Services;

namespace WheelerPhotoParlour.Views
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<PhotoItem> _photos = new();
        private bool _isLoading = false;
        private bool _isFullScreen = false;
        private bool _isCheckMode = false;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private ResizeMode _previousResizeMode;
        private double _previousLeft, _previousTop, _previousWidth, _previousHeight;

        public MainWindow()
        {
            InitializeComponent();
            PhotoList.ItemsSource = _photos;

            // 先加载配置再渲染界面，避免语言切换后重开闪现中文
            ConfigService.Load();
            LocalizationService.RestoreFromConfig();

            CheckRuntime();

            LocalizationService.LanguageChanged += ApplyLocalizedText;
            ApplyLocalizedText();
        }

        // ============ 本地化 ============

        private static string T(string key) => LocalizationService.T(key);

        private void OnLangClick(object sender, RoutedEventArgs e)
        {
            LocalizationService.ToggleLanguage();
        }

        private void ApplyLocalizedText()
        {
            Title = LocalizationService.CurrentLanguage == AppLanguage.Chinese
                ? "惠勒照相馆 Wheeler Photo Parlour"
                : "Wheeler Photo Parlour 惠勒照相馆";

            AppTitleText.Text = T("AppTitle");
            AboutBtn.Content = T("AboutBtn");
            AboutBtn.ToolTip = T("AboutBtnTip");
            LangBtn.Content = T("LangBtn");

            PhotoListTitleText.Text = T("PhotoListTitle");
            SelectAllCheck.Content = T("SelectAllCheck");
            LoadingText.Text = T("StatusExporting");

            ExportAllBtn.Content = T("ExportBtn");
            ExportAllBtn.ToolTip = T("ExportBtnTip");
            ExportCheckedBtn.Content = _isCheckMode ? T("ExportCheckedBtnActive") : T("ExportCheckedBtn");
            ExportCheckedBtn.ToolTip = T("ExportCheckedBtnTip");
            ExportSelectedBtn.Content = T("ExportSelectedBtn");
            ExportSelectedBtn.ToolTip = T("ExportSelectedBtnTip");
            DeleteBtn.Content = T("DeleteBtn");
            DeleteBtn.ToolTip = T("DeleteBtnTip");
            RefreshBtn.Content = T("AutoScanBtn");
            RefreshBtn.ToolTip = T("AutoScanBtnTip");
            SelectSourceBtn.Content = T("ManualPathBtn");
            SelectSourceBtn.ToolTip = T("ManualPathBtnTip");

            RefreshPreviewTexts();
            StatusText.Text = _isCheckMode ? T("CheckModeHint") : T("StatusReady");
            PhotoCountText.Text = string.Format(T("PhotoCountText"), _photos.Count);
            UpdateCheckedCountText();
        }

        private void RefreshPreviewTexts()
        {
            if (PhotoList.SelectedItem is PhotoItem item)
            {
                PhotoInfoText.Text = BuildPhotoInfoText(item);
                ApplyMetaExtraText(item);
            }
            else
            {
                EmptyPreviewText.Text = T("EmptyPreviewTitle");
                PhotoInfoText.Text = T("NoPhotoSelected");
                PhotoMetaExtraText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>文件名 + 地点（如果有），方便核对地点解析是否成功。</summary>
        private static string BuildPhotoInfoText(PhotoItem item)
        {
            return string.IsNullOrWhiteSpace(item.LocationTitle)
                ? item.FileName
                : $"{item.FileName}　📍{item.LocationTitle}";
        }

        /// <summary>游戏内时间填到第二行；没有就收起。</summary>
        private void ApplyMetaExtraText(PhotoItem item)
        {
            var parts = new List<string>();
            if (item.GameDateTime.HasValue)
            {
                parts.Add($"{T("GameDateTimeLabel")}：{item.GameDateTime.Value:yyyy-MM-dd HH:mm:ss}");
            }

            if (parts.Count == 0)
            {
                PhotoMetaExtraText.Visibility = Visibility.Collapsed;
                PhotoMetaExtraText.Text = "";
            }
            else
            {
                PhotoMetaExtraText.Text = string.Join("　", parts);
                PhotoMetaExtraText.Visibility = Visibility.Visible;
            }
        }

        // ============ 全屏切换 ============

        private void OnWindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F11)
            {
                ToggleFullScreen();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape && _isFullScreen)
            {
                ToggleFullScreen();
                e.Handled = true;
            }
        }

        private void ToggleFullScreen()
        {
            if (!_isFullScreen)
            {
                // 记住原始窗口状态，退出时还原
                _previousWindowState = WindowState;
                _previousWindowStyle = WindowStyle;
                _previousResizeMode = ResizeMode;

                // 如果当前是最大化，先恢复Normal再取尺寸，否则会把最大化尺寸当成"正常"尺寸
                // 导致退出全屏后点还原按钮时窗口变得巨大
                if (_previousWindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }

                _previousLeft = Left;
                _previousTop = Top;
                _previousWidth = Width;
                _previousHeight = Height;

                // 进入全屏
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;

                // 用物理屏幕尺寸覆盖窗口，包含任务栏区域
                // （Maximized只铺满工作区域，盖不住任务栏）
                Left = 0;
                Top = 0;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;

                Topmost = true;

                _isFullScreen = true;
            }
            else
            {
                Topmost = false;

                WindowStyle = _previousWindowStyle;
                ResizeMode = _previousResizeMode;
                Left = _previousLeft;
                Top = _previousTop;
                Width = _previousWidth;
                Height = _previousHeight;
                WindowState = _previousWindowState;

                _isFullScreen = false;
            }
        }

        // ============ 运行时检测 ============

        private void CheckRuntime()
        {
            try
            {
                var runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
                if (string.IsNullOrEmpty(runtime) || !runtime.Contains(".NET"))
                {
                    ShowRuntimeWarning();
                }
            }
            catch { }
        }

        private void ShowRuntimeWarning()
        {
            var result = System.Windows.MessageBox.Show(
                T("RuntimeWarningMessage"),
                T("RuntimeWarningTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://dotnet.microsoft.com/download/dotnet/8.0",
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }

        // ============ 启动 / 关闭 ============

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            StatusText.Text = T("StatusInitializing");
            LoadLogo();
            PhotoService.InitCache();

            if (ConfigService.Config.FirstRun)
            {
                TryAutoDetectPath();
                ConfigService.Config.FirstRun = false;
            }

            _ = LoadPhotosAsync();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            ConfigService.Save();
            PhotoService.ClearCache();
        }

        private void LoadLogo()
        {
            try
            {
                var exePath = AppDomain.CurrentDomain.BaseDirectory;
                var iconPath = Path.Combine(exePath, "app_icon.jpg");
                if (File.Exists(iconPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmap.UriSource = new Uri(iconPath);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    LogoImage.Source = bitmap;
                }
            }
            catch { }
        }

        private void TryAutoDetectPath()
        {
            try
            {
                var dirs = Directory.GetDirectories(ConfigService.Config.SourcePath);
                if (dirs.Length > 0)
                {
                    ConfigService.Config.SourcePath = dirs[0];
                }
            }
            catch { }
        }

        // ============ 加载照片 ============

        private async Task LoadPhotosAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            if (_isCheckMode) ExitCheckMode();

            StatusText.Text = T("StatusScanning");
            _photos.Clear();

            try
            {
                var sourcePath = ConfigService.Config.SourcePath;
                if (!Directory.Exists(sourcePath))
                {
                    StatusText.Text = T("StatusSourceMissing");
                    _isLoading = false;
                    ShowNoPhotosFoundDialog();
                    return;
                }

                var files = await Task.Run(() =>
                {
                    return Directory.GetFiles(sourcePath)
                        .Where(f => Path.GetFileName(f).StartsWith("PRDR"))
                        .OrderByDescending(f => File.GetCreationTime(f))
                        .ToList();
                });

                PhotoService.InitCache();

                foreach (var file in files)
                {
                    var cachePath = PhotoService.GetCachePath(file);
                    if (!File.Exists(cachePath))
                    {
                        await Task.Run(() => PhotoService.ConvertToJpg(file, cachePath));
                    }

                    var meta = await Task.Run(() => PhotoService.ExtractMeta(file));

                    var item = new PhotoItem
                    {
                        SourcePath = file,
                        CachePath = cachePath,
                        FileName = Path.GetFileName(file),
                        FileSize = new FileInfo(file).Length,
                        LocationTitle = meta.Title,
                        GameDateTime = meta.GameDateTime,
                        RealWorldDateTime = meta.RealWorldDateTime,
                        CreatedTime = GetTrustworthyRealTime(file)
                    };

                    _ = item.LoadThumbnailAsync();
                    _photos.Add(item);
                }

                PhotoCountText.Text = string.Format(T("PhotoCountText"), _photos.Count);
                ExportAllBtn.IsEnabled = _photos.Count > 0;
                ExportCheckedBtn.IsEnabled = _photos.Count > 0;
                SelectAllCheck.IsChecked = false;
                UpdateCheckedCountText();

                if (_photos.Count > 0)
                {
                    StatusText.Text = T("StatusReady");
                    System.Windows.MessageBox.Show(
                        string.Format(T("ScanSuccessTitle"), _photos.Count) + "\n\n" + T("ScanSuccessSubtitle"),
                        T("InfoTitle"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    StatusText.Text = T("StatusReady");
                    ShowNoPhotosFoundDialog();
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = string.Format(T("StatusLoadFailedFormat"), ex.Message);
            }

            _isLoading = false;
        }

        /// <summary>
        /// 获取文件的可信真实创建时间，优先用创建时间，不可用则退回最后写入时间。
        /// </summary>
        private static DateTime GetTrustworthyRealTime(string filePath)
        {
            try
            {
                var creationTime = File.GetCreationTime(filePath);
                var lastWriteTime = File.GetLastWriteTime(filePath);

                // 如果创建时间合理（不是默认值），优先使用
                if (creationTime != default && creationTime.Year >= 2010)
                    return creationTime;

                // 否则退回到最后写入时间
                if (lastWriteTime != default && lastWriteTime.Year >= 2010)
                    return lastWriteTime;
            }
            catch { }

            return File.GetCreationTime(filePath);
        }

        private void ShowNoPhotosFoundDialog()
        {
            System.Windows.MessageBox.Show(
                T("NoPhotosFoundTitle") + "\n\n" + T("NoPhotosFoundSubtitle"),
                T("WarningTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        // ============ 预览 ============

        private async void OnPhotoSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PhotoList.SelectedItem is PhotoItem item)
            {
                EmptyPanel.Visibility = Visibility.Collapsed;
                PhotoPreview.Visibility = Visibility.Visible;
                LoadingPanel.Visibility = Visibility.Visible;

                BitmapImage? bitmap = null;
                try
                {
                    bitmap = await PhotoService.LoadFullImageAsync(item.CachePath);
                }
                catch
                {
                    // 文件损坏，静默处理
                }

                LoadingPanel.Visibility = Visibility.Collapsed;

                if (bitmap == null)
                {
                    PhotoPreview.Visibility = Visibility.Collapsed;
                    EmptyPanel.Visibility = Visibility.Visible;
                    System.Windows.MessageBox.Show(
                        T("FileCorruptedTitle"),
                        T("ErrorTitle"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                PhotoPreview.Source = bitmap;
                ExportSelectedBtn.IsEnabled = true;
                DeleteBtn.IsEnabled = true;
                PhotoInfoText.Text = BuildPhotoInfoText(item);
                PhotoSizeText.Text = item.FileSizeText;
                ApplyMetaExtraText(item);
            }
            else
            {
                EmptyPanel.Visibility = Visibility.Visible;
                PhotoPreview.Visibility = Visibility.Collapsed;
                LoadingPanel.Visibility = Visibility.Collapsed;
                PhotoPreview.Source = null;

                ExportSelectedBtn.IsEnabled = false;
                DeleteBtn.IsEnabled = false;
                PhotoInfoText.Text = T("NoPhotoSelected");
                PhotoSizeText.Text = "";
                PhotoMetaExtraText.Visibility = Visibility.Collapsed;
            }
        }

        // ============ 多选勾选 ============

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            bool selectAll = SelectAllCheck.IsChecked == true;
            foreach (var photo in _photos)
            {
                photo.IsSelected = selectAll;
            }
            UpdateCheckedCountText();
        }

        private void OnThumbClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 非勾选模式时，点击缩略图只切换预览，不处理勾选
            if (!_isCheckMode) return;

            if (sender is FrameworkElement element && element.DataContext is PhotoItem item)
            {
                item.IsSelected = !item.IsSelected;
                UpdateCheckedCountText();

                // 若未全部勾选，取消全选框
                if (_photos.Count > 0 && _photos.Any(p => !p.IsSelected))
                {
                    SelectAllCheck.IsChecked = false;
                }
            }

            // 勾选模式下点击只用于勾选，不切换预览
            e.Handled = true;
        }

        private void UpdateCheckedCountText()
        {
            var checkedCount = _photos.Count(p => p.IsSelected);
            SelectedCountText.Text = checkedCount > 0
                ? string.Format(T("SelectedCountText"), checkedCount)
                : "";
        }

        // ============ 导出全部 ============

        private void OnExportAllClick(object sender, RoutedEventArgs e)
        {
            if (_photos.Count == 0) return;

            var dialog = new ExportModeDialog { Owner = this };
            if (dialog.ShowDialog() != true) return;

            if (dialog.ExportAsZip)
                ExportAsZip(_photos.ToList(), dialog.GroupByLocation, dialog.TimestampMode);
            else
                ExportToFolder(_photos.ToList(), dialog.GroupByLocation, dialog.TimestampMode);
        }

        // ============ 自选冲印 ============

        private void OnExportCheckedClick(object sender, RoutedEventArgs e)
        {
            if (!_isCheckMode)
            {
                EnterCheckMode();
                return;
            }

            // 已在勾选模式：退出并导出已勾选项
            var checkedPhotos = _photos.Where(p => p.IsSelected).ToList();
            ExitCheckMode();

            if (checkedPhotos.Count == 0) return;

            var dialog = new ExportModeDialog { Owner = this };
            if (dialog.ShowDialog() != true) return;

            if (dialog.ExportAsZip)
                ExportAsZip(checkedPhotos, dialog.GroupByLocation, dialog.TimestampMode);
            else
                ExportToFolder(checkedPhotos, dialog.GroupByLocation, dialog.TimestampMode);
        }

        private void EnterCheckMode()
        {
            _isCheckMode = true;
            ExportCheckedBtn.Content = T("ExportCheckedBtnActive");
            ExportCheckedBtn.Style = (Style)FindResource("PrimaryButton");
            SelectAllCheck.Visibility = Visibility.Visible;
            StatusText.Text = T("CheckModeHint");

            // 禁用其余按钮，避免误触
            ExportAllBtn.IsEnabled = false;
            ExportSelectedBtn.IsEnabled = false;
            DeleteBtn.IsEnabled = false;
            RefreshBtn.IsEnabled = false;
            SelectSourceBtn.IsEnabled = false;
        }

        private void ExitCheckMode()
        {
            _isCheckMode = false;
            ExportCheckedBtn.Content = T("ExportCheckedBtn");
            ExportCheckedBtn.Style = (Style)FindResource("AppButton");
            SelectAllCheck.Visibility = Visibility.Collapsed;

            // 清空勾选，避免残留
            foreach (var photo in _photos)
            {
                photo.IsSelected = false;
            }
            SelectAllCheck.IsChecked = false;
            UpdateCheckedCountText();
            StatusText.Text = T("StatusReady");

            // 按各按钮原本的可用性条件还原，不一律设为true
            ExportAllBtn.IsEnabled = _photos.Count > 0;
            RefreshBtn.IsEnabled = true;
            SelectSourceBtn.IsEnabled = true;
            bool hasPreviewSelection = PhotoList.SelectedItem is PhotoItem;
            ExportSelectedBtn.IsEnabled = hasPreviewSelection;
            DeleteBtn.IsEnabled = hasPreviewSelection;
        }

        // ============ 导出命名 ============

        /// <summary>
        /// 生成导出目标路径。命名格式"地点_时间.jpg"，地点解析失败回退为占位名。
        /// 同批次内重名自动加 _2、_3 后缀防止覆盖。
        /// </summary>
        private Dictionary<PhotoItem, string> BuildExportNames(List<PhotoItem> photos, bool groupByLocation, bool useZipSeparator, string timestampMode)
        {
            var unknownLabel = T("UnknownLocationFolder");
            var usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new Dictionary<PhotoItem, string>();
            var separator = useZipSeparator ? "/" : Path.DirectorySeparatorChar.ToString();

            foreach (var item in photos)
            {
                var dir = groupByLocation ? item.GetLocationFolderName(unknownLabel) : "";
                var baseName = item.GetExportBaseName(unknownLabel, timestampMode);

                var finalName = baseName;
                var counter = 2;
                while (!usedKeys.Add($"{dir}{separator}{finalName}".ToLowerInvariant()))
                {
                    finalName = $"{baseName}_{counter}";
                    counter++;
                }

                var relativePath = string.IsNullOrEmpty(dir) ? $"{finalName}.jpg" : $"{dir}{separator}{finalName}.jpg";
                result[item] = relativePath;
            }

            return result;
        }

        private void ExportAsZip(List<PhotoItem> photos, bool groupByLocation, string timestampMode)
        {
            using var sfd = new SaveFileDialog
            {
                Title = T("SaveZipTitle"),
                FileName = string.Format(T("SaveZipDefaultName"), DateTime.Now.ToString("yyyyMMdd_HHmmss")),
                Filter = T("SaveZipFilter")
            };

            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            if (!IsExportPathValid(sfd.FileName))
            {
                ShowInvalidExportPathDialog();
                return;
            }

            var entryNameByItem = BuildExportNames(photos, groupByLocation, useZipSeparator: true, timestampMode);

            var total = photos.Count;
            var success = 0;
            var fail = 0;

            try
            {
                // 确保 ZIP 在 MessageBox 弹出前已关闭
                using (var archive = ZipFile.Open(sfd.FileName, ZipArchiveMode.Create))
                {
                    for (int i = 0; i < photos.Count; i++)
                    {
                        var item = photos[i];
                        StatusText.Text = T("StatusExporting") + $" ({i + 1}/{total})";
                        System.Windows.Forms.Application.DoEvents();

                        try
                        {
                            archive.CreateEntryFromFile(item.CachePath, entryNameByItem[item]);
                            success++;
                        }
                        catch { fail++; }
                    }
                }

                // ZIP 已关闭，可以安全弹窗
                ApplyRealCaptureTimestamp(photos, timestampMode, sfd.FileName);

                StatusText.Text = T("StatusReady");
                string zipSummary = LocalizationService.CurrentLanguage == AppLanguage.Chinese
                    ? $"成功：{success} 帧 / 失手：{fail} 帧\n安放之处：{sfd.FileName}"
                    : $"Succeeded: {success} / Failed: {fail}\nDelivered to: {sfd.FileName}";
                System.Windows.MessageBox.Show(
                    T("ExportSuccessTitle") + "\n\n" + T("ExportSuccessSubtitle") + "\n\n" + zipSummary,
                    T("ExportResultTitle"),
                    MessageBoxButton.OK,
                    fail > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

                if (success > 0)
                {
                    OpenExplorerAndSelectFile(sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = T("StatusReady");
                System.Windows.MessageBox.Show(
                    string.Format(T("ExportFailedMessage"), ex.Message),
                    T("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToFolder(List<PhotoItem> photos, bool groupByLocation, string timestampMode)
        {
            using var fbd = new FolderBrowserDialog { Description = T("SelectFolderTitle") };
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            if (!Directory.Exists(fbd.SelectedPath))
            {
                ShowInvalidExportPathDialog();
                return;
            }

            var relativePathByItem = BuildExportNames(photos, groupByLocation, useZipSeparator: false, timestampMode);

            var total = photos.Count;
            var success = 0;
            var fail = 0;
            var overwrite = false;

            var existing = photos
                .Select(p => Path.Combine(fbd.SelectedPath, relativePathByItem[p]))
                .Where(File.Exists)
                .ToList();

            if (existing.Count > 0)
            {
                var result = System.Windows.MessageBox.Show(
                    string.Format(T("OverwriteConfirmMessage"), existing.Count),
                    T("OverwriteConfirmTitle"),
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel) return;
                overwrite = result == MessageBoxResult.Yes;
            }

            for (int i = 0; i < photos.Count; i++)
            {
                var item = photos[i];
                StatusText.Text = T("StatusExporting") + $" ({i + 1}/{total})";
                System.Windows.Forms.Application.DoEvents();

                try
                {
                    var target = Path.Combine(fbd.SelectedPath, relativePathByItem[item]);
                    var targetDir = Path.GetDirectoryName(target);
                    if (!string.IsNullOrEmpty(targetDir)) Directory.CreateDirectory(targetDir);

                    File.Copy(item.CachePath, target, overwrite);
                    success++;
                }
                catch { fail++; }
            }

            ApplyRealCaptureTimestamp(photos, timestampMode, fbd.SelectedPath);

            StatusText.Text = T("StatusReady");
            string folderSummary = LocalizationService.CurrentLanguage == AppLanguage.Chinese
                ? $"成功：{success} 帧 / 失手：{fail} 帧\n安放之处：{fbd.SelectedPath}"
                : $"Succeeded: {success} / Failed: {fail}\nDelivered to: {fbd.SelectedPath}";
            System.Windows.MessageBox.Show(
                T("ExportSuccessTitle") + "\n\n" + T("ExportSuccessSubtitle") + "\n\n" + folderSummary,
                T("ExportResultTitle"),
                MessageBoxButton.OK,
                fail > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

            if (success > 0)
            {
                OpenExplorerFolder(fbd.SelectedPath);
            }
        }

        /// <summary>
        /// 将真实拍摄时间写入导出文件的文件系统时间戳。
        /// </summary>
        private static void ApplyRealCaptureTimestamp(List<PhotoItem> photos, string timestampMode, string basePath)
        {
            if (timestampMode != "RealTime") return;

            foreach (var item in photos)
            {
                var timestamp = GetTimestampForExport(item, timestampMode);
                if (timestamp == null) continue;

                try
                {
                    // ZIP导出时basePath是ZIP文件路径，无法设内部条目时间戳
                    // 文件夹导出时basePath是目标文件夹
                    if (Directory.Exists(basePath))
                    {
                        // 查找对应的导出文件
                        var exportedFiles = Directory.GetFiles(basePath, "*.jpg", System.IO.SearchOption.AllDirectories);
                        foreach (var exportedFile in exportedFiles)
                        {
                            var fi = new FileInfo(exportedFile);
                            if (fi.CreationTime > DateTime.Now.AddMinutes(5)) continue;
                            File.SetCreationTime(exportedFile, timestamp.Value);
                            break;
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>根据时间戳模式获取导出时间戳。</summary>
        private static DateTime? GetTimestampForExport(PhotoItem item, string timestampMode)
        {
            if (timestampMode == "RealTime")
            {
                return item.RealWorldDateTime ?? item.CreatedTime;
            }
            return item.GameDateTime ?? item.CreatedTime;
        }

        private void OnExportSelectedClick(object sender, RoutedEventArgs e)
        {
            if (PhotoList.SelectedItem is not PhotoItem item) return;

            // 先弹出 ExportModeDialog（单帧模式），让用户选时间戳来源
            var dialog = new ExportModeDialog { Owner = this, IsSingleMode = true };
            if (dialog.ShowDialog() != true) return;

            var defaultName = item.GetExportBaseName(T("UnknownLocationFolder"), dialog.TimestampMode) + ".jpg";

            using var sfd = new SaveFileDialog
            {
                Title = T("SaveSingleTitle"),
                FileName = defaultName,
                Filter = T("SaveSingleFilter")
            };

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!IsExportPathValid(sfd.FileName))
                {
                    ShowInvalidExportPathDialog();
                    return;
                }

                try
                {
                    File.Copy(item.CachePath, sfd.FileName, true);

                    // 如果用户选了真实世界时间，设置导出文件的创建时间
                    var timestamp = GetTimestampForExport(item, dialog.TimestampMode);
                    if (timestamp != null && dialog.TimestampMode == "RealTime")
                    {
                        try { File.SetCreationTime(sfd.FileName, timestamp.Value); } catch { }
                    }

                    System.Windows.MessageBox.Show(
                        T("ExportSuccessTitle") + "\n\n" + T("ExportSuccessSubtitle"),
                        T("InfoTitle"), MessageBoxButton.OK, MessageBoxImage.Information);

                    OpenExplorerAndSelectFile(sfd.FileName);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        string.Format(T("ExportSingleFailed"), ex.Message),
                        T("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static bool IsExportPathValid(string fullFilePath)
        {
            try
            {
                var dir = Path.GetDirectoryName(fullFilePath);
                return !string.IsNullOrEmpty(dir) && Directory.Exists(dir);
            }
            catch
            {
                return false;
            }
        }

        private void ShowInvalidExportPathDialog()
        {
            System.Windows.MessageBox.Show(
                T("InvalidExportPathTitle") + "\n\n" + T("InvalidExportPathSubtitle"),
                T("WarningTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        /// <summary>打开资源管理器并选中指定文件。</summary>
        private static void OpenExplorerAndSelectFile(string fullFilePath)
        {
            try
            {
                if (!File.Exists(fullFilePath)) return;
                Process.Start("explorer.exe", $"/select,\"{fullFilePath}\"");
            }
            catch
            {
                // 非Windows环境静默忽略
            }
        }

        /// <summary>打开指定文件夹。</summary>
        private static void OpenExplorerFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath)) return;
                Process.Start("explorer.exe", $"\"{folderPath}\"");
            }
            catch
            {
                // 静默忽略
            }
        }

        // ============ 删除单张 ============

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (PhotoList.SelectedItem is not PhotoItem item) return;

            var result = System.Windows.MessageBox.Show(
                string.Format(T("DeleteConfirmMessage"), item.FileName),
                T("DeleteConfirmTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.OK) return;

            try
            {
                FileSystem.DeleteFile(item.SourcePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                if (File.Exists(item.CachePath)) File.Delete(item.CachePath);

                _photos.Remove(item);
                PhotoCountText.Text = string.Format(T("PhotoCountText"), _photos.Count);
                ExportAllBtn.IsEnabled = _photos.Count > 0;
                ExportCheckedBtn.IsEnabled = _photos.Count > 0;
                UpdateCheckedCountText();

                if (_photos.Count == 0 && _isCheckMode) ExitCheckMode();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    string.Format(T("DeleteFailedMessage"), ex.Message),
                    T("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============ 自动扫描 / 重新扫描 ============

        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await LoadPhotosAsync();
        }

        // ============ 手动选择存档路径 ============

        private async void OnSelectSourceClick(object sender, RoutedEventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = T("SelectSourceFolderTitle"),
                InitialDirectory = ConfigService.Config.SourcePath
            };

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ConfigService.Config.SourcePath = fbd.SelectedPath;
                await LoadPhotosAsync();
            }
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            new AboutWindow { Owner = this }.ShowDialog();
        }
    }
}
