using Machine_Performance_Management.Main;
using System.Windows;

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
            mainWindowViewModel = new MainWindowViewModel(username, role, fullname)
            {
                UserName = username,
                Role = role,
                FullName = fullname
            };
            DataContext = mainWindowViewModel;
        }
    }
}
