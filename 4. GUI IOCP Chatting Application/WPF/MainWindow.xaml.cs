using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Minimize and Close Button Events
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Friends_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TogglePanelButton_Click(object sender, RoutedEventArgs e)
        {
            // 패널이 표시되면 숨기고, 숨겨져 있으면 표시
            InternalPanel.Visibility = InternalPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ClosePanelButton_Click(object sender, RoutedEventArgs e)
        {
            // 닫기 버튼으로 패널 숨기기
            InternalPanel.Visibility = Visibility.Collapsed;
        }
    }
}