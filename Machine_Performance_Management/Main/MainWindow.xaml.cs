using Machine_Performance_Management.Login;
using Machine_Performance_Management.Main;
using OfficeOpenXml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Machine_Performance_Management.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel mainWindowViewModel;
        public MainWindow(string username, string role, string fullname)
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialPersonal("abc");

            mainWindowViewModel = new MainWindowViewModel(username, role, fullname)
            {
                UserName = username,
                Role = role,
                FullName = fullname
            };
            DataContext = mainWindowViewModel;
        }

        private void Expander_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Expander expander)
            {
                expander.IsExpanded = !expander.IsExpanded;
            }
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
		{
            LoginView loginWindow = new LoginView(); // Tạo LoginView mới
            loginWindow.Show(); // Hiển thị LoginView

            this.Close(); // Đóng MainWindow
        }
	}
}
