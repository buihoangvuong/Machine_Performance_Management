using Machine_Performance_Management.Extension;
using Microsoft.Win32;
using MySqlConnector;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Machine_Performance_Management.Login
{
    public class LoginModel
    {
        protected LocalConfigTable config;

        public LoginModel()
        {
            IniService ini = new IniService();
            config = ini.GetLocalConfig();
        }
        public string GetLatestVersion()
        {
            string latestVersion = null;
            string query = "SELECT version_name FROM version ORDER BY event_time DESC, version_name DESC LIMIT 1";

            try
            {
                using (DbService db = new DbService(config))
                using (var cmd = db.GetMySqlCommand(query))
                using (var reader = cmd.ExecuteReader()) // ✅ Đọc dữ liệu
                {
                    if (reader.Read())
                    {
                        latestVersion = reader.GetString(0); // ✅ Lấy theo index (nhanh hơn)
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy phiên bản mới nhất: {ex.Message}");
            }

            return latestVersion; // Trả về null nếu không có dữ liệu
        }

        public async Task<(LoginResult, string, UserInfo)> LoginUserAsync(string username, string password)
        {
            string query = "SELECT user_name FROM user_profile WHERE user_name=@username";

            try
            {
                using (DbService db = new DbService(config))
                using (var cmd = db.GetMySqlCommand(query))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            return (LoginResult.InvalidUsername, "Account does not exist \n Tài khoản không tồn tại!", null);
                        }
                    }
                }

                // Nếu user tồn tại, kiểm tra mật khẩu
                query = "SELECT user_id, user_name, password, role, full_name FROM user_profile WHERE user_name=@username AND password=@password";

                using (DbService db = new DbService(config))
                using (var cmd = db.GetMySqlCommand(query))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            return (LoginResult.InvalidPassword, "Incorrect password \n Mật khẩu không đúng!", null);
                        }

                        // Lấy thông tin user
                        var userInfo = new UserInfo
                        {
                            UserId = reader.GetInt32("user_id"),
                            UserName = reader.GetString("user_name"),
                            Password = reader.GetString("password"),
                            Role = reader.GetString("role"),
                            Fullname = reader.GetString("full_name")
                        };

                        return (LoginResult.Success, "", userInfo);
                    }
                }
            }
            catch (MySqlException ex)
            {
                return (LoginResult.DatabaseError, $"Error connecting to server \n Lỗi kết nối đến máy chủ!: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                return (LoginResult.DatabaseError, $"Lỗi hệ thống: {ex.Message}", null);
            }
        }
    }
    public static class PasswordHelper
    {
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordHelper),
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPassword =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static string GetBoundPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(BoundPassword);
        }

        public static void SetBoundPassword(DependencyObject obj, string value)
        {
            obj.SetValue(BoundPassword, value);
        }

        public static bool GetBindPassword(DependencyObject obj)
        {
            return (bool)obj.GetValue(BindPassword);
        }

        public static void SetBindPassword(DependencyObject obj, bool value)
        {
            obj.SetValue(BindPassword, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox && !passwordBox.Password.Equals(e.NewValue))
            {
                passwordBox.Password = (string)e.NewValue;
            }
        }

        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetBoundPassword(passwordBox, passwordBox.Password);
            }
        }
    }
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public static class RegistryHelper
    {
        private const string SubKey = "SOFTWARE\\LEDMesTool";

        public static void Write(string key, string value)
        {
            using (var registryKey = Registry.CurrentUser.CreateSubKey(SubKey))
            {
                registryKey?.SetValue(key, value);
            }
        }

        public static string Read(string key)
        {
            using (var registryKey = Registry.CurrentUser.OpenSubKey(SubKey))
            {
                return registryKey?.GetValue(key)?.ToString() ?? "";
            }
        }
    }
}
