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

            // 必须先加载配置、恢复上次选择的语言，再渲染界面文字，
            // 否则界面会先按默认中文渲染一次，造成"切换英文后重开还是中文"的假象。
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
                PhotoInfoText.Text = item.FileName;
            }
            else
            {
                EmptyPreviewText.Text = T("EmptyPreviewTitle");
                PhotoInfoText.Text = T("NoPhotoSelected");
            }
        }

        // ============ 全屏切换（F11） ============

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
                // 进入全屏前记住原始窗口状态与位置尺寸，方便退出时还原
                _previousWindowState = WindowState;
                _previousWindowStyle = WindowStyle;
                _previousResizeMode = ResizeMode;
                _previousLeft = Left;
                _previousTop = Top;
                _previousWidth = Width;
                _previousHeight = Height;

                // 先恢复到Normal状态再手动指定位置和尺寸，避免Maximized状态下Left/Top/Width/Height设置不生效
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;

                // 用整个物理屏幕尺寸（包含任务栏区域）覆盖窗口，而不是WindowState.Maximized
                // 后者只会铺满"工作区域"（屏幕减去任务栏），无法真正盖住任务栏
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

        // ============ 加载照片（搜寻马背上的相机袋） ============

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

                    var item = new PhotoItem
                    {
                        SourcePath = file,
                        CachePath = cachePath,
                        FileName = Path.GetFileName(file),
                        FileSize = new FileInfo(file).Length
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
                    // 损坏文件读取失败时静默处理，下方按 null 结果展示报错提示
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
                PhotoInfoText.Text = item.FileName;
                PhotoSizeText.Text = item.FileSizeText;
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
            }
        }

        // ============ 多选勾选（自选冲印用） ============

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
            // 不在勾选模式时，点缩略图只用于切换右侧预览，不处理勾选，也不拦截事件冒泡
            if (!_isCheckMode) return;

            if (sender is FrameworkElement element && element.DataContext is PhotoItem item)
            {
                item.IsSelected = !item.IsSelected;
                UpdateCheckedCountText();

                // 若不是全部都勾选了，"全选"框应自动取消勾选，避免显示不一致
                if (_photos.Count > 0 && _photos.Any(p => !p.IsSelected))
                {
                    SelectAllCheck.IsChecked = false;
                }
            }

            // 勾选模式下，点击只用于勾选，不切换右侧预览，避免误操作
            e.Handled = true;
        }

        private void UpdateCheckedCountText()
        {
            var checkedCount = _photos.Count(p => p.IsSelected);
            SelectedCountText.Text = checkedCount > 0
                ? string.Format(T("SelectedCountText"), checkedCount)
                : "";
        }

        // ============ 导出全部（打包付梓） ============

        private void OnExportAllClick(object sender, RoutedEventArgs e)
        {
            if (_photos.Count == 0) return;

            var dialog = new ExportModeDialog { Owner = this };
            if (dialog.ShowDialog() != true) return;

            if (dialog.ExportAsZip)
                ExportAsZip(_photos.ToList());
            else
                ExportToFolder(_photos.ToList());
        }

        // ============ 自选冲印（勾选模式开关） ============

        private void OnExportCheckedClick(object sender, RoutedEventArgs e)
        {
            if (!_isCheckMode)
            {
                EnterCheckMode();
                return;
            }

            // 已在勾选模式下再次点击：退出勾选模式，若有勾选项则弹出导出方式询问
            var checkedPhotos = _photos.Where(p => p.IsSelected).ToList();
            ExitCheckMode();

            if (checkedPhotos.Count == 0) return;

            var dialog = new ExportModeDialog { Owner = this };
            if (dialog.ShowDialog() != true) return;

            if (dialog.ExportAsZip)
                ExportAsZip(checkedPhotos);
            else
                ExportToFolder(checkedPhotos);
        }

        private void EnterCheckMode()
        {
            _isCheckMode = true;
            ExportCheckedBtn.Content = T("ExportCheckedBtnActive");
            ExportCheckedBtn.Style = (Style)FindResource("PrimaryButton");
            SelectAllCheck.Visibility = Visibility.Visible;
            StatusText.Text = T("CheckModeHint");

            // 禁用其余操作按钮，避免在自选挑选过程中误触导致整批导出/删除等意外操作
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

            // 退出勾选模式时清空所有勾选状态，避免下次进入模式时残留上次的选择造成困惑
            foreach (var photo in _photos)
            {
                photo.IsSelected = false;
            }
            SelectAllCheck.IsChecked = false;
            UpdateCheckedCountText();
            StatusText.Text = T("StatusReady");

            // 恢复被禁用的按钮，各按钮按其原本的可用性条件还原，而非一律设为true
            ExportAllBtn.IsEnabled = _photos.Count > 0;
            RefreshBtn.IsEnabled = true;
            SelectSourceBtn.IsEnabled = true;
            bool hasPreviewSelection = PhotoList.SelectedItem is PhotoItem;
            ExportSelectedBtn.IsEnabled = hasPreviewSelection;
            DeleteBtn.IsEnabled = hasPreviewSelection;
        }

        private void ExportAsZip(List<PhotoItem> photos)
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

            var total = photos.Count;
            var success = 0;
            var fail = 0;

            try
            {
                using var archive = ZipFile.Open(sfd.FileName, ZipArchiveMode.Create);
                for (int i = 0; i < photos.Count; i++)
                {
                    var item = photos[i];
                    StatusText.Text = T("StatusExporting") + $" ({i + 1}/{total})";
                    System.Windows.Forms.Application.DoEvents();

                    try
                    {
                        archive.CreateEntryFromFile(item.CachePath, Path.GetFileName(item.CachePath));
                        success++;
                    }
                    catch { fail++; }
                }

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

        private void ExportToFolder(List<PhotoItem> photos)
        {
            using var fbd = new FolderBrowserDialog { Description = T("SelectFolderTitle") };
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            if (!Directory.Exists(fbd.SelectedPath))
            {
                ShowInvalidExportPathDialog();
                return;
            }

            var total = photos.Count;
            var success = 0;
            var fail = 0;
            var overwrite = false;

            var existing = photos
                .Select(p => Path.Combine(fbd.SelectedPath, Path.GetFileName(p.CachePath)))
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
                    var target = Path.Combine(fbd.SelectedPath, Path.GetFileName(item.CachePath));
                    File.Copy(item.CachePath, target, overwrite);
                    success++;
                }
                catch { fail++; }
            }

            StatusText.Text = T("StatusReady");
            string folderSummary = LocalizationService.CurrentLanguage == AppLanguage.Chinese
                ? $"成功：{success} 帧 / 失手：{fail} 帧"
                : $"Succeeded: {success} / Failed: {fail}";
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

        private void OnExportSelectedClick(object sender, RoutedEventArgs e)
        {
            if (PhotoList.SelectedItem is not PhotoItem item) return;

            using var sfd = new SaveFileDialog
            {
                Title = T("SaveSingleTitle"),
                FileName = Path.GetFileName(item.CachePath),
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

        /// <summary>打开文件资源管理器并选中指定文件，让用户能立刻看到刚导出的成果</summary>
        private static void OpenExplorerAndSelectFile(string fullFilePath)
        {
            try
            {
                if (!File.Exists(fullFilePath)) return;
                Process.Start("explorer.exe", $"/select,\"{fullFilePath}\"");
            }
            catch
            {
                // 打开资源管理器失败（例如非Windows环境）时静默忽略，不影响导出本身已经成功的结果
            }
        }

        /// <summary>打开指定文件夹，用于"逐帧导出到文件夹"完成后让用户直接看到所有导出文件</summary>
        private static void OpenExplorerFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath)) return;
                Process.Start("explorer.exe", $"\"{folderPath}\"");
            }
            catch
            {
                // 同上，静默忽略
            }
        }

        // ============ 删除单张（焚毁此帧） ============

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
