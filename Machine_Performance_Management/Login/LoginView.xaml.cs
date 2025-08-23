using System.Windows;
using System.Windows.Input;

namespace Machine_Performance_Management.Login
{

    public partial class LoginView : Window
    {
        private readonly LoginViewModel loginViewModel = new LoginViewModel();
        public LoginView()
        {
            InitializeComponent();
            DataContext = loginViewModel;
        }

        private void txtPassword_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(UsernameTextBox.Text))
            //{
            //    Keyboard.Focus(PasswordBox);
            //}
        }

        private void UsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
	}
}
