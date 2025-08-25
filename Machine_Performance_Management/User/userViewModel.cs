using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine_Performance_Management.Admin;
using Machine_Performance_Management.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static Machine_Performance_Management.User.userModel;

namespace Machine_Performance_Management.User
{
    public class userViewModel : ObservableObject
    {

        public userViewModel(string fullname)
        {
            Fullname = fullname;


            #region Usermanagement
            LoadUserData();
            SelectedItemDetails = new ObservableCollection<userPasswordModel>
                {
                    new userPasswordModel("Username", ""),
                    new userPasswordModel("Password", ""),
                    new userPasswordModel("Role", ""),
                    new userPasswordModel("Fullname", ""),
                };
            
            #endregion

        }
        protected userModel userModel = new userModel();


        #region User_frofile

        protected userModel userManagerModel = new userModel();
        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string> { "MANAGER", "STAFF" };
        public ObservableCollection<userPasswordModel> _selectedItemDetails { get; set; } = new ObservableCollection<userPasswordModel>();
        public ObservableCollection<userPasswordModel> SelectedItemDetails
        {
            get => _selectedItemDetails;
            set
            {
                _selectedItemDetails = value;
                OnPropertyChanged("StandardInforEditItem");
            }
        }

        public ObservableCollection<UserProfile> userProfiles { get; set; } = new ObservableCollection<UserProfile>();

        public ObservableCollection<UserProfile> UserData
        {
            get => userProfiles;
            set
            {
                userProfiles = value;
                OnPropertyChanged("UserData");
            }
        }
        private UserProfile _selectedUser;
        public UserProfile SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    Username = value.UserName;
                    Password = value.PassWord;
                    Rolename = value.Role;
                    Fullname = value.FullName;
                }
                UpdateSelectedItemDetails();
            }
        }
        private void UpdateSelectedItemDetails()
        {

            if (SelectedUser != null)
            {
                SelectedItemDetails[0].Value = SelectedUser.UserName;
                SelectedItemDetails[1].Value = SelectedUser.PassWord;
                SelectedItemDetails[2].Value = SelectedUser.Role;
                SelectedItemDetails[3].Value = SelectedUser.FullName;

                OnPropertyChanged(nameof(SelectedItemDetails));
            }
        }

        private string username;
        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        private string password;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private string rolename;
        public string Rolename
        {
            get => rolename;
            set
            {
                rolename = value;
                OnPropertyChanged("Rolename");
            }
        }

        private string fullname;
        public string Fullname
        {
            get => fullname;
            set
            {
                fullname = value;
                OnPropertyChanged("Fullname");
            }
        }



        public void LoadUserData()
        {
            var userdatas = userManagerModel.LoadUserProfiles();

            UserData.Clear();
            foreach (var userdata in userdatas)
            {
                UserData.Add(userdata);
            }
        }


        private ICommand clickButtonAddCommand;
        public ICommand AddCommand => clickButtonAddCommand ?? (clickButtonAddCommand = new RelayCommand(ClickAddButton));

        private ICommand clickButtonEditCommand;
        public ICommand EditCommand => clickButtonEditCommand ?? (clickButtonEditCommand = new RelayCommand(ClickEditButton));

        private ICommand clickButtonDeleteCommand;
        public ICommand DeleteCommand => clickButtonDeleteCommand ?? (clickButtonDeleteCommand = new RelayCommand(ClickDeleteButton));

        public void ClickAddButton()
        {
            string user = SelectedItemDetails[0].Value;
            string password = SelectedItemDetails[1].Value;
            string role = SelectedItemDetails[2].Value;
            string fullname = SelectedItemDetails[3].Value;

            if (!ValidateUsername(user) || !ValidatePassword(password) || !ValidateRole(role) || !ValidateFullName(fullname))
            {
                return;
            }

            if (userManagerModel.CheckUserExists(user))
            {
                MessageBox.Show("Username already exists, please choose another username!\nTên người dùng đã tồn tại, vui lòng chọn tên khác!", "Thông báo / Notification", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            userManagerModel.AddUser(user, password, role, fullname);
            LoadUserData();
            InitValue();
            ClearSelectedItemDetails();
        }

        public void ClickEditButton()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Please select a user to edit!\nVui lòng chọn người dùng để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string newUsername = SelectedItemDetails[0].Value;
            string newPassword = SelectedItemDetails[1].Value;
            string newRole = SelectedItemDetails[2].Value;
            string newFullname = SelectedItemDetails[3].Value;
            int userID = _selectedUser.UserId;

            // Kiểm tra dữ liệu hợp lệ
            if (!ValidateUsername(newUsername) || !ValidatePassword(newPassword) || !ValidateRole(newRole) || !ValidateFullName(newFullname))
            {
                return;
            }

            // Kiểm tra thông tin có thay đổi không
            bool isChanged =
                !string.Equals(newUsername, SelectedUser.UserName, StringComparison.Ordinal) ||
                !string.Equals(newPassword, SelectedUser.PassWord, StringComparison.Ordinal) ||
                !string.Equals(newRole.ToLower(), SelectedUser.Role.ToLower(), StringComparison.Ordinal) ||
                !string.Equals(newFullname, SelectedUser.FullName, StringComparison.Ordinal);

            if (!isChanged)
            {
                MessageBox.Show("You have not changed any information!\nBạn chưa thay đổi thông tin nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Kiểm tra trùng Username
            if (!string.Equals(newUsername, SelectedUser.UserName, StringComparison.Ordinal) &&
                userManagerModel.CheckUserExists(newUsername))
            {
                MessageBox.Show("Username already exists, please choose another one!\nTên người dùng đã tồn tại, vui lòng chọn tên khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                // Thực hiện cập nhật
                userManagerModel.UpdateUser(userID, newUsername, newPassword, newRole, newFullname);

                MessageBox.Show("Information updated successfully!\nCập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload lại dữ liệu & reset
                LoadUserData();
                InitValue();
                ClearSelectedItemDetails();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating.\nĐã xảy ra lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ClickDeleteButton()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Please select a user to delete!\nVui lòng chọn một người dùng để xóa!",
                "Notification / Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete user \"{SelectedUser.UserName}\"?\nBạn có chắc chắn muốn xóa người dùng này không?",
                "Xác nhận | Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                userManagerModel.DeleteUser(SelectedUser.UserName);

                MessageBox.Show("User deleted successfully!\nNgười dùng đã được xóa thành công!",
                "Success / Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadUserData();
                InitValue();
                ClearSelectedItemDetails();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi xóa người dùng: {ex.Message}",
                "Lỗi / Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void InitValue()
        {
            Username = "";
            Password = "";
            Rolename = "";
            Fullname = "";
        }

        private bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Tên người dùng không được để trống!\nUsername cannot be empty!", "Thông báo / Notification", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Mật khẩu không được để trống!\nPassword cannot be empty!", "Thông báo / Notification", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool ValidateRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Vui lòng chọn vai trò!\nPlease select a role!", "Thông báo / Notification", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool ValidateFullName(string fullname)
        {
            if (string.IsNullOrWhiteSpace(fullname))
            {
                MessageBox.Show("Họ và tên không được để trống!\nFullname cannot be empty!", "Thông báo / Notification", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        private void ClearSelectedItemDetails()
        {
            SelectedItemDetails[0].Value = "";
            SelectedItemDetails[1].Value = "";
            SelectedItemDetails[2].Value = "";
            SelectedItemDetails[3].Value = "";
            SelectedUser = null;
        }

        public class RoleVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value != null && value.ToString() == "Role" ? Visibility.Visible : Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Binding.DoNothing;
            }
        }

        #endregion
    }
}
