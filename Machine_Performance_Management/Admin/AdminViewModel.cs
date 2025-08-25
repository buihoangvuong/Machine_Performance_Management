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

            LoadFactoryItems();
            GetLoadDatadeviceType();
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
            var DeviceList = adminModel.LoadDatadeviceType();
            DeviceTypeData.Clear();
            foreach (var item in DeviceList)
            {
                DeviceTypeData.Add(item);
            }
        }
        public void LoadFactoryItems()
        {
            Factoryitem.Clear();
           
            var list = adminModel.GetFactoryList();

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

            if (adminModel.CheckDeviceNameExists(devicename, factory))
            {
                MessageBox.Show("Device already exists, please choose another username!\nThiết bị đã tồn tại, vui lòng chọn tên khác!", "Thông báo / Notification", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int newProductId;
            if (adminModel.InsertDevicetype(devicename, devicetype, description, factory, Fullname, out newProductId))
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
                adminModel.CheckDeviceNameExists(devicename, factory))
            {
                MessageBox.Show("Device name already exists, please choose another one!\nThiết bị đã tồn tại, vui lòng chọn tên khác!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                // Thực hiện cập nhật
                int Id = SelectedDeviceTypeInfo.id;

                // Gọi cập nhật
                bool isUpdated = adminModel.UpdateDeviceType(Id, devicename, devicetype, description, factory, Fullname);

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


                adminModel.DeleteDeviceType(Id);
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