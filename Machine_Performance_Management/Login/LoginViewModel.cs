using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine_Performance_Management.Extension;
using Machine_Performance_Management.Main;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Machine_Performance_Management.Login
{
    public partial class LoginViewModel : ObservableObject
    {
        private const string CurrentVersion = "0.0.1"; // Phiên bản hiện tại của tool
        protected LoginModel loginModel = new LoginModel();
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        private string password;
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }
        private bool rememberMe;
        public bool RememberMe
        {
            get => rememberMe;
            set => SetProperty(ref rememberMe, value);
        }
        private ICommand ClickBtnLogin;
        public ICommand LoginCommand => ClickBtnLogin ?? (ClickBtnLogin = new AsyncRelayCommand(LoginAsync));
        public LoginViewModel()
        {
            CheckVersionAsync();
            Username = RegistryHelper.Read("Username");
            RememberMe = !string.IsNullOrEmpty(Username);
        }
        private void CheckVersionAsync()
        {
            try
            {
                var latestVersion = loginModel.GetLatestVersion();

                if (!string.IsNullOrEmpty(latestVersion) && latestVersion != CurrentVersion)
                {
                    MessageBox.Show($"Phiên bản mới ({latestVersion}) đã có sẵn! Vui lòng cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi kiểm tra version / phiên bản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

			var (loginResult, errorMessage, userInfo) = await loginModel.LoginUserAsync(Username, Password);

			if (loginResult != LoginResult.Success)
			{
				MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (RememberMe)
			{
				RegistryHelper.Write("Username", Username);
			}
			else
			{
				RegistryHelper.Write("Username", "");
			}
			//Đăng nhập thành công -> Chuyển sang MainWindow
		   MainWindow main = new MainWindow(userInfo.UserName, userInfo.Role, userInfo.Fullname)
		   {
			   DataContext = new MainWindowViewModel(userInfo.UserName, userInfo.Role, userInfo.Fullname)
		   };
			main.Show();
			Application.Current.MainWindow.Close();
		}
    }
}
