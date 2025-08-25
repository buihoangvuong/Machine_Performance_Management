using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine_Performance_Management.Admin;
using Machine_Performance_Management.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Machine_Performance_Management.Factory
{
    public class FactoryViewModel : ObservableObject
    {

        public FactoryViewModel(string fullname)
        {
            Fullname = fullname;


            #region Usermanagement
            GetLoadFactory();
        
            SelectedFactoryDetails = new ObservableCollection<FactoryInforEdit>
               {
                   new FactoryInforEdit{ FactoryEdit ="Factory Name",Value = "" }, // Tên cho phần tử 0
                   new FactoryInforEdit{FactoryEdit ="Description" ,Value = ""},// Tên cho phần tử 1

               };
           
            #endregion

        }

        public enum FactoryName
        {
            efactory_name,
            edescription,
        }

        protected FactoryModel factoryModel = new FactoryModel();
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

        public class FactoryInforEdit : ObservableObject
        {
            public string FactoryEdit { get; set; }
            private string _value;
            public string Value
            {
                get => _value;
                set
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }

        }

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
            var FactoryList = factoryModel.LoadFactoryList();
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

            if (factoryModel.IsFactoryNameExists(name))
            {
                MessageBox.Show("Factory name already exists. Please enter another name.\nTên nhà máy đã tồn tại. Vui lòng nhập tên khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (factoryModel.AddFactory(name, description, Fullname, out int newFactoryId))
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
            if (factoryModel.IsFactoryNameExists(name, SelectedFactory.Factory_id))
            {
                MessageBox.Show("Factory name already exists. Please enter another name.\nTên nhà máy đã tồn tại. Vui lòng nhập tên khác.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (factoryModel.UpdateFactory(SelectedFactory.Factory_id, name, description, Fullname))
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
                if (factoryModel.DeleteFactory(SelectedFactory.Factory_id))
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

    }
}
