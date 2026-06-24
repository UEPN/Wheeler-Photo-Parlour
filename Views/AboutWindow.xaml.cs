using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WheelerPhotoParlour.Services;

namespace WheelerPhotoParlour.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LoadLogo();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyLocalizedText();
        }

        private static string T(string key) => LocalizationService.T(key);

        private void ApplyLocalizedText()
        {
            bool isChinese = LocalizationService.CurrentLanguage == AppLanguage.Chinese;

            Title = T("AboutWindowTitle");
            TitleTextBlock.Text = T("AppTitle");
            VersionTextBlock.Text = "Version 1.1.0";

            DesignLabelText.Text = isChinese ? "项目发起与设计：" : "Concept & Design:";
            CodeHostLabelText.Text = isChinese ? "代码托管：" : "Code Hosting:";
            RepoLabelText.Text = isChinese ? "项目地址：" : "Repository:";
            LicenseLabelText.Text = isChinese ? "开源许可：" : "License:";

            CreditBodyText.Text = T("AboutCredit");

            DisclaimerHeaderText.Text = isChinese ? "免责声明" : "Disclaimer";
            DisclaimerBodyText.Text = T("AboutDisclaimer");
            CopyrightText.Text = "© 2026 UEPN & Novilune. Licensed under the GNU General Public License v3.0.";

            OkBtn.Content = T("AboutOkBtn");
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
                    AboutLogo.Source = bitmap;
                }
            }
            catch { }
        }

        /// <summary>点击链接时用系统默认浏览器打开。</summary>
        private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
            catch
            {
                // 调起浏览器失败，静默忽略
            }

            e.Handled = true;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
