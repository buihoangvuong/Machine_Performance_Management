using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine_Performance_Management.Common;
using Machine_Performance_Management.Extension;
using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Machine_Performance_Management.Admin
{
    public class AdminViewModel : ObservableObject
    {
        public AdminViewModel(string fullname)
        {
            Fullname = fullname;


            #region Usermanagement
            LoadUserData();
            LoadFactoryItems();
            GetLoadFactory();
            GetLoadDatadeviceType();
            SelectedItemDetails = new ObservableCollection<userPasswordModel>
                {
                    new userPasswordModel("Username", ""),
                    new userPasswordModel("Password", ""),
                    new userPasswordModel("Role", ""),
                    new userPasswordModel("Fullname", ""),
                };
            SelectedFactoryDetails = new ObservableCollection<FactoryInforEdit>
               {
                   new FactoryInforEdit{ FactoryEdit ="Factory Name",Value = "" }, // Tên cho phần tử 0
                   new FactoryInforEdit{FactoryEdit ="Description" ,Value = ""},// Tên cho phần tử 1

               };
            DeviceTypeEditItem = new ObservableCollection<DeviceTypeEdit>
                {
                   new DeviceTypeEdit { DeviceTypeName = "Machine Type", Value = "" },
                   new DeviceTypeEdit { DeviceTypeName = "Machine Name", Value = "" },
                   new DeviceTypeEdit { DeviceTypeName = "Description", Value = "" },
                   new DeviceTypeEdit { DeviceTypeName = "Factory", Value = "" },
                };
            #endregion

        }
        protected AdminModel adminModel = new AdminModel();
        
        
        #region User_frofile

        protected AdminModel userManagerModel = new AdminModel();
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
        #region Factory
        private ObservableCollection<FactoryInfo> _factoryData { get; set; } = new ObservableCollection<FactoryInfo>();
        public ObservableCollection<FactoryInfo> FactoryData
        {
            get => _factoryData;
            set
            {
                _factoryData = value;
                OnPropertyChanged("FactoryData");
            }
        }
        private ObservableCollection<FactoryInforEdit> _selectedFactoryDetails { get; set; } = new ObservableCollection<FactoryInforEdit>();
        public ObservableCollection<FactoryInforEdit> SelectedFactoryDetails
        {
            get => _selectedFactoryDetails;
            set
            {
                _selectedFactoryDetails = value;
                OnPropertyChanged("SelectedFactoryDetails");
            }
        }
        private FactoryInfo _selectedFactory;
        public FactoryInfo SelectedFactory
        {
            get => _selectedFactory;
            set
            {
                if (SetProperty(ref _selectedFactory, value))
                {
                    if (_selectedFactory != null && _selectedFactoryDetails.Count >= 2)
                    {
                        _selectedFactoryDetails[(int)FactoryName.efactory_name].Value = _selectedFactory.FactoryName ?? "";
                        _selectedFactoryDetails[(int)FactoryName.edescription].Value = _selectedFactory.Description ?? "";
                    }
                }
            }
        }


        public void GetLoadFactory()
        {
            var FactoryList = userManagerModel.LoadFactoryList();
            FactoryData.Clear();
            foreach (var item in FactoryList)
            {
                FactoryData.Add(item);
            }
        }
        private ICommand _createFactoryCommand;
        public ICommand AddFactoryCommand => _createFactoryCommand ?? (_createFactoryCommand = new RelayCommand(ClickAddFactoryButton));

        private ICommand _updateFactoryCommand;
        public ICommand EditactoryCommand => _updateFactoryCommand ?? (_updateFactoryCommand = new RelayCommand(ClickEditactoryButton));

        private ICommand _deleteFactoryCommand;
        public ICommand DeleteFactoryCommand => _deleteFactoryCommand ?? (_deleteFactoryCommand = new RelayCommand(ClickDeleteFactoryButton));

        private void ClickAddFactoryButton()
        {
            string name = SelectedFactoryDetails[(int)FactoryName.efactory_name].Value;
            string description = SelectedFactoryDetails[(int)FactoryName.edescription].Value;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter factory name.\nVui lòng nhập tên nhà máy.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (userManagerModel.IsFactoryNameExists(name))
            {
                MessageBox.Show("Factory name already exists. Please enter another name.\nTên nhà máy đã tồn tại. Vui lòng nhập tên khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (userManagerModel.AddFactory(name, description, Fullname, out int newFactoryId))
            {
                MessageBox.Show("Add factory successfully\nThêm nhà máy thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                GetLoadFactory();
                ClearFactoryDetails();
            }
            else
            {
                MessageBox.Show("Add factory successfully\nThêm nhà máy thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ClickEditactoryButton()
        {
            if (SelectedFactory == null)
            {
                MessageBox.Show("Please select a factory to update\nVui lòng chọn một nhà máy để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string name = SelectedFactoryDetails[(int)FactoryName.efactory_name].Value;
            string description = SelectedFactoryDetails[(int)FactoryName.edescription].Value;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter factory name.\nVui lòng nhập tên nhà máy.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (name == SelectedFactory.FactoryName && description == SelectedFactory.Description)
            {
                MessageBox.Show("You have not changed any information.\n Bạn chưa thay đổi thông tin nào.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // Kiểm tra tên trùng, nhưng loại trừ chính nhà máy đang chỉnh sửa
            if (userManagerModel.IsFactoryNameExists(name, SelectedFactory.Factory_id))
            {
                MessageBox.Show("Factory name already exists. Please enter another name.\nTên nhà máy đã tồn tại. Vui lòng nhập tên khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (userManagerModel.UpdateFactory(SelectedFactory.Factory_id, name, description, Fullname))
            {
                MessageBox.Show("Factory update successful!\nCập nhật nhà máy thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                GetLoadFactory();
                ClearFactoryDetails();
            }
            else
            {
                MessageBox.Show("Factory update failed!\nCập nhật nhà máy thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickDeleteFactoryButton()
        {
            if (SelectedFactory == null)
            {
                MessageBox.Show("Please select a factory to delete.\nVui lòng chọn một nhà máy để xoá.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("\"Are you sure you want to delete this factory?\nBạn có chắc chắn muốn xoá nhà máy này không?", "Xác nhận xoá", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (userManagerModel.DeleteFactory(SelectedFactory.Factory_id))
                {
                    MessageBox.Show("Factory deleted successfully!\nXoá nhà máy thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    GetLoadFactory();
                    ClearFactoryDetails();
                }
            }
        }
        private void ClearFactoryDetails()
        {
            foreach (var item in SelectedFactoryDetails)
            {
                item.Value = string.Empty;
            }

            SelectedFactory = null; // Hủy chọn dòng đang chọn
        }
        #endregion
        #region Machine Type
        // reload khi chuyển sang tab machine info
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged();

                    if (_selectedTabIndex == 2) // Tab "Machine Info"
                    {
                        GetLoadDatadeviceType();
                        LoadFactoryItems();
                        ResetDevicetypeItem();
                    }
                }
            }
        }

        private ObservableCollection<Device_Type> _deviceTypeData { get; set; } = new ObservableCollection<Device_Type>();
        public ObservableCollection<Device_Type> DeviceTypeData
        {
            get => _deviceTypeData;
            set
            {
                _deviceTypeData = value;
                OnPropertyChanged("DeviceTypeData");
            }
        }

        private ObservableCollection<string> _factoryitem = new ObservableCollection<string>();
        public ObservableCollection<string> Factoryitem
        {
            get => _factoryitem;
            set
            {
                _factoryitem = value;
                OnPropertyChanged(nameof(Factoryitem));
            }
        }

        private string _selectedFactoryItem;
        public string SelectedFactoryItem
        {
            get => _selectedFactoryItem;
            set
            {
                _selectedFactoryItem = value;
                OnPropertyChanged(nameof(SelectedFactoryItem));                
                GetLoadDatadeviceType();

            }
        }
        private ObservableCollection<DeviceTypeEdit> _deviceTypeEditItem { get; set; } = new ObservableCollection<DeviceTypeEdit>();
        public ObservableCollection<DeviceTypeEdit> DeviceTypeEditItem
        {
            get => _deviceTypeEditItem;
            set
            {
                _deviceTypeEditItem = value;
                OnPropertyChanged("DeviceTypeEditItem");
            }
        }

        private Device_Type _selectedDeviceTypeInfo;
        public Device_Type SelectedDeviceTypeInfo
        {
            get => _selectedDeviceTypeInfo;
            set
            {
                if (SetProperty(ref _selectedDeviceTypeInfo, value, nameof(SelectedDeviceTypeInfo)))
                {
                    if (_selectedDeviceTypeInfo != null && _deviceTypeEditItem.Count >= 3)
                    {
                        _deviceTypeEditItem[(int)DeviceTypeName.edevice_type].Value = _selectedDeviceTypeInfo.Device_type ?? "";
                        _deviceTypeEditItem[(int)DeviceTypeName.edevice_name].Value = _selectedDeviceTypeInfo.Device_Name ?? "";
                        _deviceTypeEditItem[(int)DeviceTypeName.edescription].Value = _selectedDeviceTypeInfo.Description ?? "";
                        _deviceTypeEditItem[(int)DeviceTypeName.efactory].Value = _selectedDeviceTypeInfo.Factory ?? "";

                        OnPropertyChanged(nameof(_deviceTypeEditItem));

                    }
                }
            }
        }

        private ICommand _createDeviceTypeCommand;
        public ICommand CreateDeviceTypeCommand => _createDeviceTypeCommand ?? (_createDeviceTypeCommand = new RelayCommand(ClickCreateDeviceTypeButton));

        private ICommand _updateDeviceTypeCommand;
        public ICommand UpdateDeviceTypeCommand => _updateDeviceTypeCommand ?? (_updateDeviceTypeCommand = new RelayCommand(ClickUpdateDeviceTypeButton));

        private ICommand _deleteDeviceTypeCommand;
        public ICommand DeleteDeviceTypeCommand => _deleteDeviceTypeCommand ?? (_deleteDeviceTypeCommand = new RelayCommand(ClickDeleteDeviceTypeButton));

        public void GetLoadDatadeviceType()
        {
            var DeviceList = userManagerModel.LoadDatadeviceType();
            DeviceTypeData.Clear();
            foreach (var item in DeviceList)
            {
                DeviceTypeData.Add(item);
            }
        }
        public void LoadFactoryItems()
        {
            Factoryitem.Clear();
           
            var list = userManagerModel.GetFactoryList();

            foreach (var item in list)
            {
                Factoryitem.Add(item);
               
            }
            // Auto-select the first item
            if (Factoryitem.Count > 0)
                SelectedFactoryItem = Factoryitem[0];
        

        }
        public void ClickCreateDeviceTypeButton()
        {
            string devicetype = DeviceTypeEditItem[0].Value?.Trim();
            string devicename = DeviceTypeEditItem[1].Value?.Trim();
            string description = DeviceTypeEditItem[2].Value?.Trim();
            string factory = DeviceTypeEditItem[3].Value?.Trim();

            if (string.IsNullOrWhiteSpace(devicetype) || string.IsNullOrWhiteSpace(devicename) || string.IsNullOrWhiteSpace(factory))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (userManagerModel.CheckDeviceNameExists(devicename, factory))
            {
                MessageBox.Show("Device already exists, please choose another username!\nThiết bị đã tồn tại, vui lòng chọn tên khác!", "Thông báo / Notification", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int newProductId;
            if (userManagerModel.InsertDevicetype(devicename, devicetype, description, factory, Fullname, out newProductId))
            {
                MessageBox.Show("Add device type successfully\nThêm nhà máy thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetDevicetypeItem();
                GetLoadDatadeviceType();
            }
            else
            {
                MessageBox.Show("Device Type is invalid!\nloại dữ liệu sản phẩm không hợp lệ!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ClickUpdateDeviceTypeButton()
        {
            if (SelectedDeviceTypeInfo == null)
            {
                MessageBox.Show("Please select an Device type before updating!\nVui lòng chọn Loại dữ liệu sản phẩm trước khi cập nhật!",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string devicetype = DeviceTypeEditItem[0].Value?.Trim();
            string devicename = DeviceTypeEditItem[1].Value?.Trim();
            string description = DeviceTypeEditItem[2].Value?.Trim();
            string factory = DeviceTypeEditItem[3].Value?.Trim();

            bool isChanged =
               !string.Equals(devicetype, SelectedDeviceTypeInfo.Device_type, StringComparison.Ordinal) ||
               !string.Equals(description, SelectedDeviceTypeInfo.Description, StringComparison.Ordinal) ||
               !string.Equals(devicename, SelectedDeviceTypeInfo.Device_Name, StringComparison.Ordinal) ||
               !string.Equals(factory.ToLower(), SelectedDeviceTypeInfo.Factory.ToLower(), StringComparison.Ordinal);

            if (!isChanged)
            {
                MessageBox.Show("You have not changed any information!\nBạn chưa thay đổi thông tin nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(devicetype) || string.IsNullOrWhiteSpace(devicename) || string.IsNullOrWhiteSpace(factory))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            bool isDeviceNameChanged = !string.Equals(devicename, SelectedDeviceTypeInfo.Device_Name, StringComparison.Ordinal);
            bool isFactoryChanged = !string.Equals(factory?.Trim().ToLower(), SelectedDeviceTypeInfo.Factory?.Trim().ToLower(), StringComparison.Ordinal);

            if ((isDeviceNameChanged || isFactoryChanged) &&
                userManagerModel.CheckDeviceNameExists(devicename, factory))
            {
                MessageBox.Show("Device name already exists, please choose another one!\nThiết bị đã tồn tại, vui lòng chọn tên khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                // Thực hiện cập nhật
                int Id = SelectedDeviceTypeInfo.id;

                // Gọi cập nhật
                bool isUpdated = userManagerModel.UpdateDeviceType(Id, devicename, devicetype, description, factory, Fullname);

                if (isUpdated)
                {
                    GetLoadDatadeviceType();
                    MessageBox.Show("Edit data success!\nChỉnh sửa dữ liệu thành công!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetDevicetypeItem();
                }
                else
                {
                    MessageBox.Show("Update DEvice Type failed!\nCập nhật loại thiết bị thất bại", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating.\nĐã xảy ra lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ClickDeleteDeviceTypeButton()
        {
            if (SelectedDeviceTypeInfo == null)
            {
                MessageBox.Show("Please select a Device before Delete!\nVui lòng chọn thiết bị trước khi xóa!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int Id = SelectedDeviceTypeInfo.id;
            string devicetype = DeviceTypeEditItem[0].Value?.Trim();
            string devicename = DeviceTypeEditItem[1].Value?.Trim();
            string description = DeviceTypeEditItem[2].Value?.Trim();
            string factory = DeviceTypeEditItem[3].Value?.Trim();


            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this Device Type?\nBạn có chắc chắn muốn xóa loại thiết bị này không?",
                                                      "Warning",
                                                      MessageBoxButton.OKCancel,
                                                      MessageBoxImage.Warning);
            if (result == MessageBoxResult.OK)
            {


                userManagerModel.DeleteDeviceType(Id);
                ResetDevicetypeItem();
                GetLoadDatadeviceType();
            }
        }
        private void ResetDevicetypeItem()
        {
            for (int i = 0; i < _deviceTypeEditItem.Count; i++)
            {
                _deviceTypeEditItem[i].Value = string.Empty;
            }
            SelectedDeviceTypeInfo = null;
            OnPropertyChanged(nameof(_deviceTypeEditItem));
        }

        #endregion    
    }
}