using System.Windows;
using WheelerPhotoParlour.Services;

namespace WheelerPhotoParlour.Views
{
    public partial class ExportModeDialog : Window
    {
        public bool ExportAsZip => ZipRadio.IsChecked == true;

        public ExportModeDialog()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            string T(string key) => LocalizationService.T(key);

            Title = T("ExportModeTitle");
            TitleText.Text = T("ExportModeTitle");
            ZipRadio.Content = T("ExportAsZipOption");
            FolderRadio.Content = T("ExportToFolderOption");
            OkBtn.Content = T("DialogOk");
            CancelBtn.Content = T("DialogCancel");
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
