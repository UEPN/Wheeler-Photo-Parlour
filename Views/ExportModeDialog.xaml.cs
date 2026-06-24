using System.Windows;
using WheelerPhotoParlour.Services;

namespace WheelerPhotoParlour.Views
{
    public partial class ExportModeDialog : Window
    {
        public bool ExportAsZip => ZipRadio.IsChecked == true;
        public bool GroupByLocation => GroupByLocationCheck.IsChecked == true;
        public string TimestampMode => TimestampGameRadio.IsChecked == true ? "GameTime" : "RealTime";
        public bool IsSingleMode { get; set; }

        public ExportModeDialog()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            string T(string key) => LocalizationService.T(key);

            if (IsSingleMode)
            {
                Title = T("ExportModeTitleSingle");
                TitleText.Text = T("ExportModeTitleSingle");
                ZipRadio.Visibility = Visibility.Collapsed;
                FolderRadio.Visibility = Visibility.Collapsed;
                GroupByLocationCheck.Visibility = Visibility.Collapsed;
            }
            else
            {
                Title = T("ExportModeTitle");
                TitleText.Text = T("ExportModeTitle");
                ZipRadio.Content = T("ExportAsZipOption");
                FolderRadio.Content = T("ExportToFolderOption");
                GroupByLocationCheck.Content = T("GroupByLocationOption");
                GroupByLocationCheck.ToolTip = T("GroupByLocationOptionTip");
            }

            TimestampLabel.Text = T("TimestampSourceLabel");
            TimestampRealRadio.Content = T("TimestampRealOption");
            TimestampRealRadio.ToolTip = T("TimestampRealOptionTip");
            TimestampGameRadio.Content = T("TimestampGameOption");
            TimestampGameRadio.ToolTip = T("TimestampGameOptionTip");

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
