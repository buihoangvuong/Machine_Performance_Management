using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine_Performance_Management.Admin;
using Machine_Performance_Management.Home;
using Machine_Performance_Management.Login;
using Machine_Performance_Management.Performance;
using System.Windows;
using System.Windows.Input;
namespace Machine_Performance_Management.Main
{
	public class MainWindowViewModel : ObservableObject
    {
		#region khai báo
		private bool _showUserStaff;
        private bool _showUserManager;
        public bool ShowUserStaff
        {
            get => _showUserStaff;
            set => SetProperty(ref _showUserStaff, value);
        }
        public bool ShowUserManager
        {
            get => _showUserManager;
            set => SetProperty(ref _showUserManager, value);
        }
        private string _selectedMenuItem;
        public string SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                _selectedMenuItem = value;
                OnPropertyChanged();
            }
        }
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        private string _username;
        public string UserName
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged("UserName");
            }
        }

        private string _fullname;

        public string FullName
        {
            get => _fullname;
            set
            {
                _fullname = value;
                OnPropertyChanged("FullName");
            }
        }

        private string _role;
        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged("Role");
            }
        }
		#endregion

		private ICommand _clickHome;
        public ICommand ClickHome => _clickHome ?? (_clickHome = new RelayCommand(MenuHome));

        private ICommand _clickPerformance;
        public ICommand ClickPerformance => _clickPerformance ?? (_clickPerformance = new RelayCommand(MenuPerformance));

        private ICommand _clickAdmin;
        public ICommand ClickAdmin => _clickAdmin ?? (_clickAdmin = new RelayCommand(MenuAdmin));

        private ICommand _clickLogOut;
        public ICommand ClickLogOut => _clickLogOut ?? (_clickLogOut = new RelayCommand(MenuLogOut));
        public MainWindowViewModel(string username, string role, string fullname)
        {
            UserName = username;
            Role = role;
            FullName = fullname;
            MenuHome();
            if (Role == "MANAGER")
            {
                ShowUserManager = true;
                ShowUserStaff = true;
            }
        }

        public void MenuHome()
        {
            CurrentView = new HomeView();
            SelectedMenuItem = "Home";
        }

        public void MenuPerformance()
        {
            CurrentView = new PerformanceView(FullName);
            SelectedMenuItem = "Performance";
        }

        public void MenuAdmin()
        {
			CurrentView = new AdminView(FullName);
			SelectedMenuItem = "Admin";
		}


        public void MenuLogOut()
        {
            // Đóng MainWindow
            // Đóng toàn bộ ứng dụng
            Application.Current.Shutdown();

            // Mở LoginView (có thể được thực hiện ở một nơi khác, thường là trong App.xaml.cs)
            var loginWindow = new LoginView();
            loginWindow.Show();
        }
    }
}
