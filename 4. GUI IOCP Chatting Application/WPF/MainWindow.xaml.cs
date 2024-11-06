using System.Collections.ObjectModel;
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

    public class Friend
    {
        public string? Name { get; set; }
        public string? Status { get; set; }  // 예: 온라인/오프라인 상태
    }

    public class CustomButton : Button
    {
        public static readonly DependencyProperty CloseButtonVisibleProperty =
            DependencyProperty.Register("CloseButtonVisible", typeof(bool), typeof(CustomButton), new PropertyMetadata(false));

        public bool CloseButtonVisible
        {
            get { return (bool)GetValue(CloseButtonVisibleProperty); }
            set { SetValue(CloseButtonVisibleProperty, value); }
        }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<Friend>? Friendss { get; set; }
        private string currentPanel = "";
        bool isPlaying = false;
        bool chatingPanel = false;
        private string currentChatFriend = "";

        public MainWindow()
        {
            InitializeComponent();
            Friendss = new ObservableCollection<Friend>
            {
                new Friend { Name = "Alice", Status = "Online" },
                new Friend { Name = "Bob", Status = "Offline" },
                new Friend { Name = "Charlie", Status = "Away" },
                new Friend { Name = "David", Status = "Online" },
                new Friend { Name = "Eve", Status = "Do Not Disturb" },
                new Friend { Name = "Frank", Status = "Offline" },
                new Friend { Name = "Grace", Status = "Online" },
                new Friend { Name = "Heidi", Status = "Offline" },
                new Friend { Name = "Ivan", Status = "Online" },
                new Friend { Name = "Judy", Status = "Away" },
                new Friend { Name = "Mallory", Status = "Do Not Disturb" },
                new Friend { Name = "Niaj", Status = "Offline" },
                new Friend { Name = "Oscar", Status = "Online" },
                new Friend { Name = "Peggy", Status = "Offline" },
                new Friend { Name = "Quentin", Status = "Do Not Disturb" },
                new Friend { Name = "Rupert", Status = "Away" },
                new Friend { Name = "Sybil", Status = "Online" },
                new Friend { Name = "Trent", Status = "Offline" },
                new Friend { Name = "Uma", Status = "Online" },
                new Friend { Name = "Victor", Status = "Away" },
                new Friend { Name = "Wendy", Status = "Offline" },
                new Friend { Name = "Xavier", Status = "Online" },
                new Friend { Name = "Yvonne", Status = "Do Not Disturb" },
                new Friend { Name = "Zara", Status = "Online" }
            // 추가 친구 데이터
            };
            DataContext = this;

            //FriendsList.IsEnabled = false;
        }

        private void MyMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // 비디오가 끝나면 처음으로 돌아가서 다시 재생
            MyMediaElement.Position = TimeSpan.Zero;
            MyMediaElement.Play();
        }

        // Window controls
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
        //



        private void Friends_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Friends");
        }

        private void Group_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Group");
        }

        private void Notice_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Notice");
        }

        private void ShowPanel(string panelName)
        {
            // 현재 열려 있는 패널이 클릭된 패널과 같다면 패널을 닫고 종료
            if (currentPanel == panelName)
            {
                FriendsPanel.Visibility = Visibility.Collapsed;
                GroupPanel.Visibility = Visibility.Collapsed;
                NoticePanel.Visibility = Visibility.Collapsed;
                currentPanel = ""; // 현재 패널을 초기화
                return;
            }

            // 모든 패널을 숨김
            FriendsPanel.Visibility = Visibility.Collapsed;
            GroupPanel.Visibility = Visibility.Collapsed;
            NoticePanel.Visibility = Visibility.Collapsed;

            // 클릭한 패널만 표시
            switch (panelName)
            {
                case "Friends":
                    FriendsPanel.Visibility = Visibility.Visible;
                    break;
                case "Group":
                    GroupPanel.Visibility = Visibility.Visible;
                    break;
                case "Notice":
                    NoticePanel.Visibility = Visibility.Visible;
                    break;
            }
            currentPanel = panelName;
        }


        private void GameStart_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                MyMediaElement.Pause();
                isPlaying = !isPlaying;
            }
            else
            {
                MyMediaElement.Play();
                isPlaying = !isPlaying;
            }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.DataContext is Friend selectedFriend)
            {
                // 이미 추가된 친구인지 확인
                bool isAlreadyAdded = false;
                foreach (UIElement element in ChatElementsPanel.Children)
                {
                    if (element is Button button && button.Content.ToString() == selectedFriend.Name)
                    {
                        isAlreadyAdded = true;
                        break;
                    }
                }

                if (isAlreadyAdded) return;

                // StackPanel의 총 폭과 현재 요소의 폭을 계산
                double totalWidth = 0;
                foreach (UIElement element in ChatElementsPanel.Children)
                {
                    if (element is FrameworkElement frameworkElement)
                    {
                        totalWidth += frameworkElement.ActualWidth;
                    }
                }

                if (totalWidth + 150 >= this.ActualWidth - 200)
                {
                    if (ChatElementsPanel.Children.Count > 0)
                    {
                        ChatElementsPanel.Children.RemoveAt(ChatElementsPanel.Children.Count - 1);
                    }
                }

                // CustomButton 생성
                CustomButton chatElement = new CustomButton
                {
                    Content = $"{selectedFriend.Name}",
                    Width = 100,
                    Height = 33,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Template = (ControlTemplate)FindResource("MouseOverButtonTemplate"),
                    Tag = "#303030",
                    Margin = new Thickness(0, 0, 1, 0),
                    CloseButtonVisible = true,
                    Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255))

                };
                
                chatElement.Click += ChatElement_Click;

                // 패널에 추가
                ChatElementsPanel.Children.Add(chatElement);
            }
        }
        private void ChatElement_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CustomButton clickedButton)
            {
                string friendName = clickedButton.Content.ToString() ?? "";
                if (friendName.Equals(currentChatFriend))
                {
                    ChatPanel.Visibility = Visibility.Collapsed;
                    if (chatingPanel)
                    {
                        chatingPanel = !chatingPanel;
                    }
                    currentChatFriend = "";
                    return;
                }

                ChatHeader.Text = $"{friendName}";
                currentChatFriend = friendName ?? "";


                // 클릭된 버튼의 위치 계산
                var buttonPosition = clickedButton.TransformToAncestor(this).Transform(new Point(0, 0));

                // 채팅 패널 위치 설정 (버튼 위로 위치 조정)
                ChatPanel.Margin = new Thickness(buttonPosition.X, 0, 0, 0);

                ChatMessages.Items.Clear();

                if (!chatingPanel)
                {
                    chatingPanel = !chatingPanel;
                    ChatPanel.Visibility = Visibility.Visible;
                }
            }
        }

        // X 버튼 클릭 이벤트 핸들러
        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // 이게 있어야 뒤에 버튼 안 눌리고 버그 안 남
            // 이벤트 발생한 버튼을 찾음
            var closeButton = sender as Button;
            if (closeButton == null) return;

            // VisualTreeHelper를 사용하여 상위 CustomButton을 찾음
            var customButton = FindParent<CustomButton>(closeButton);
            if (customButton == null) return;

            // ChatElementsPanel에서 해당 버튼을 삭제
            ChatElementsPanel.Children.Remove(customButton);

            // 채팅 패널 그냥 닫아버리기
            if (chatingPanel)
            {
                chatingPanel = !chatingPanel;
                ChatPanel.Visibility = Visibility.Collapsed;
            }
        }

        // 부모 요소를 찾기 위한 헬퍼 메서드
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // 부모 요소가 존재하는 동안 탐색
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T parentAsT)
                {
                    return parentAsT;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }


        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Status.Text.Equals("● Offline"))
            {
                Status.Text = "● Online";
                Status.Foreground = Brushes.Green;
            }
            else
            {
                Status.Text = "● Offline";
                Status.Foreground = Brushes.Red;
            }
        }

        private void ClosButton_Click(object sender, RoutedEventArgs e)
        {
            currentPanel = ""; // 현재 패널을 초기화
            FriendsPanel.Visibility = Visibility.Collapsed;
            GroupPanel.Visibility = Visibility.Collapsed;
            NoticePanel.Visibility = Visibility.Collapsed;
        }
    }
}