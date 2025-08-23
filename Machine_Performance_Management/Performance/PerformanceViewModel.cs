using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine_Performance_Management.Chart;
using Machine_Performance_Management.Common;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Machine_Performance_Management.Performance
{
    public class PerformanceViewModel : ObservableObject
    {
        #region Khai báo
        protected PerformanceModel performanceModel = new PerformanceModel();

        private ObservableCollection<DevicePerformance> _perFormanceData;
        public ObservableCollection<DevicePerformance> PerFormanceData
        {
            get => _perFormanceData;
            set
            {
                _perFormanceData = value;
                OnPropertyChanged(nameof(PerFormanceData));
            }
        }

        private string fullname;
        public string Fullname
        {
            get => fullname;
            set
            {
                fullname = value;
                OnPropertyChanged(nameof(Fullname));
            }
        }
        public List<string> Dates { get; set; } = new List<string>(); // Danh sách ngày để tạo cột

        private ObservableCollection<string> _factoryitemDataManagement = new ObservableCollection<string>();
        public ObservableCollection<string> FactoryitemDataManagement
        {
            get => _factoryitemDataManagement;
            set
            {
                _factoryitemDataManagement = value;
                OnPropertyChanged(nameof(FactoryitemDataManagement));
            }
        }
        public ObservableCollection<DevicePerformance1> _dataManagement { get; set; } = new ObservableCollection<DevicePerformance1>();

        public ObservableCollection<DevicePerformance1> DataManagement
        {
            get => _dataManagement;
            set
            {
                _dataManagement = value;
                OnPropertyChanged("DataManagement");
            }
        }

        private List<string> _dateHeaders;
        public List<string> DateHeaders
        {
            get => _dateHeaders;
            set
            {
                _dateHeaders = value;
                OnPropertyChanged(nameof(DateHeaders));
            }
        }

        private string _selectedFactoryItemDataManagement;
        public string SelectedFactoryItemDataManagement
        {
            get => _selectedFactoryItemDataManagement;
            set
            {
                if (_selectedFactoryItemDataManagement != value)
                {
                    _selectedFactoryItemDataManagement = value;

                    OnPropertyChanged(nameof(SelectedFactoryItemDataManagement));
                    LoadData();

                }

            }
        }

        public event Action ImportCompleted;
        #endregion

        private ICommand clickButtonImportCommand;
        public ICommand ImportCommand => clickButtonImportCommand ?? (clickButtonImportCommand = new RelayCommand(ClickImportButton));

        private ICommand _chartClickCommand;
        public ICommand ChartClickCommand => _chartClickCommand ?? (_chartClickCommand = new RelayCommand(ClickCharttButton));


        public PerformanceViewModel(string fullname)
        {
            Fullname = fullname;
            //PerFormanceData = new ObservableCollection<DevicePerformance>();
            LoadFactoryItems();
            LoadData();
        }

        public void LoadData()
        {
            var data = performanceModel.LoadPerformanceMachineList(SelectedFactoryItemDataManagement);
            PerFormanceData = new ObservableCollection<DevicePerformance>(data);
            // DateHeaders = data
            //.SelectMany(d => d.DailyPerformance.Keys)
            //.Distinct()
            //.OrderBy(d => d)
            //.ToList();
            // ImportCompleted?.Invoke();

            PerFormanceData.Clear();
            foreach (var item in data)
            {
                PerFormanceData.Add(item);
            }
            DateHeaders = data
            .SelectMany(d => d.DailyPerformance.Keys)
            .Distinct()
            .OrderBy(d => d)
            .ToList();
            ImportCompleted?.Invoke();
            // Hiển thị thông tin của máy đầu tiên trong danh sách


        }

        public void LoadFactoryItems()
        {
            FactoryitemDataManagement.Clear();
            var list = performanceModel.GetFactoryList();

            foreach (var item in list)
            {
                FactoryitemDataManagement.Add(item);
            }

            FactoryitemDataManagement.Insert(0, "All");
            if (FactoryitemDataManagement.Count > 0)
                SelectedFactoryItemDataManagement = FactoryitemDataManagement[0];

            //SelectedFactoryItemDataManagement = Factoryitem[0];

        }

        public void ClickImportButton()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Select an Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    // gọi ReadExcelData với out
                    if (!performanceModel.ReadExcelData(filePath, out var dataList)
                        || dataList == null || dataList.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu để import", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Clear và nạp dữ liệu mới
                    PerFormanceData.Clear();
                    foreach (var item in dataList)
                    {
                        PerFormanceData.Add(item);
                    }

                    // Lấy danh sách header ngày
                    DateHeaders = dataList
                        .SelectMany(d => d.DailyPerformance.Keys)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    // Event thông báo hoàn tất
                    ImportCompleted?.Invoke();

                    // Ghi DB
                    performanceModel.InsertToDatabase(dataList, Fullname);

                    MessageBox.Show("Import thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Import thất bại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public void ClickCharttButton()
        {
   //         if (PerFormanceData != null && PerFormanceData.Any())
   //         {
   //             var detail = new ChartView(PerFormanceData); // ✅ truyền cả danh sách
   //             detail.ShowDialog();
   //         }
   //         else
			//{
   //             MessageBox.Show("Không có dữ liệu cho biểu đồ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
   //         }                
        }

        private List<DevicePerformance1> data;
    }
}
