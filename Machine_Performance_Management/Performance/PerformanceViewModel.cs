using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using Machine_Performance_Management.Chart;
using Machine_Performance_Management.Common;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                    LoadMachineTypeItems();

                }

            }
        }
        private ObservableCollection<string> _machineTypeDataItem = new ObservableCollection<string>();
        public ObservableCollection<string> MachineTypeDataItem
        {
            get => _machineTypeDataItem;
            set
            {
                _machineTypeDataItem = value;
                OnPropertyChanged(nameof(MachineTypeDataItem));
            }
        }

        private string _selectedMachineTypeItem;
        public string SelectedMachineTypeItem
        {
            get => _selectedMachineTypeItem;
            set
            {
                if (_selectedMachineTypeItem != value)
                {
                    _selectedMachineTypeItem = value;

                    OnPropertyChanged(nameof(SelectedMachineTypeItem));
                    LoadData();

                }

            }
        }

        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                SetProperty(ref _dateFrom, value);
            }
        }

        private DateTime _dateTo;
        public DateTime DateTo
        {
            get => _dateTo;
            set
            {
                SetProperty(ref _dateTo, value);
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

        private bool? _isAllSelected = false;
        public bool? IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                if (_isAllSelected != value)
                {
                    _isAllSelected = value;
                    OnPropertyChanged(nameof(IsAllSelected));

                    if (_isAllSelected.HasValue && PerFormanceData != null)
                    {
                        foreach (var item in PerFormanceData)
                            item.IsSelected = _isAllSelected.Value;
                    }
                }
            }
        }



        public event Action ImportCompleted;
        #endregion


        private ICommand clickButtonImportCommand;
        public ICommand ImportCommand => clickButtonImportCommand ?? (clickButtonImportCommand = new RelayCommand(ClickImportButton));

        public ICommand _clickSearchCommand;
        public ICommand SearchCommand => _clickSearchCommand ?? (_clickSearchCommand = new RelayCommand(SearchMachine));

        private ICommand _chartClickCommand;
        public ICommand ChartClickCommand => _chartClickCommand ?? (_chartClickCommand = new RelayCommand(ClickCharttButton));


        public PerformanceViewModel(string fullname)
        {
            Fullname = fullname;
            DateTime serverTime = (DateTime)performanceModel.GetServerTime();
            DateFrom = serverTime;
            DateTo = serverTime;
            LoadFactoryItems();
            LoadMachineTypeItems();
            LoadData();
        }

        private void SearchMachine()
        {
            var data = performanceModel.LoadPerformanceMachineList(
                SelectedFactoryItemDataManagement,
                SelectedMachineTypeItem, DateFrom, DateTo
            );
            PerFormanceData = new ObservableCollection<DevicePerformance>(data);
            WireRowSelection();
            DateHeaders = data
                .SelectMany(d => d.DailyPerformance.Keys)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        public void LoadData()
        {
            DateTime DateFrom = DateTime.Now;
            DateTime DateTo = DateTime.Now;
            var data = performanceModel.LoadPerformanceMachineList(
                SelectedFactoryItemDataManagement, SelectedMachineTypeItem, DateFrom, DateTo);

            PerFormanceData = new ObservableCollection<DevicePerformance>(data);
            WireRowSelection();
            foreach (var item in PerFormanceData)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DevicePerformance.IsSelected))
                    {
                        UpdateIsAllSelected();
                    }
                };
            }

            UpdateIsAllSelected();
        }
        private void WireRowSelection()
        {
            if (PerFormanceData == null) return;

            foreach (var item in PerFormanceData)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                item.PropertyChanged += Item_PropertyChanged;
            }

            UpdateIsAllSelected();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DevicePerformance.IsSelected))
            {
                UpdateIsAllSelected();
            }
        }

        private void UpdateIsAllSelected()
        {
            if (PerFormanceData == null || PerFormanceData.Count == 0)
            {
                IsAllSelected = false;
                return;
            }

            int selectedCount = PerFormanceData.Count(x => x.IsSelected);

            if (selectedCount == 0)
                IsAllSelected = false;
            else if (selectedCount == PerFormanceData.Count)
                IsAllSelected = true;
            else
                IsAllSelected = null; // indeterminate → giống Excel
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

        public void LoadMachineTypeItems()
        {
            MachineTypeDataItem.Clear();
            var list = performanceModel.GetMachineTypeList(SelectedFactoryItemDataManagement);

            foreach (var item in list)
            {
                MachineTypeDataItem.Add(item);
            }
            MachineTypeDataItem.Insert(0, "All");
            if (MachineTypeDataItem.Count > 0)
                SelectedMachineTypeItem = MachineTypeDataItem[0];

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
            if (PerFormanceData == null || !PerFormanceData.Any(x => x.IsSelected))
            {
                MessageBox.Show("Không có dữ liệu cho biểu đồ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Lấy các dòng được chọn
            var selectedRows = PerFormanceData.Where(x => x.IsSelected).ToList();

            // Tìm ngày cuối cùng
            string lastDate = null;
            foreach (var row in selectedRows)
            {
                if (row.DailyPerformance != null && row.DailyPerformance.Any())
                {
                    var rowLast = row.DailyPerformance.Keys.Max(); // ngày cuối cùng theo thứ tự key
                    if (lastDate == null || string.Compare(rowLast, lastDate) > 0)
                        lastDate = rowLast;
                }
            }

            if (lastDate == null)
            {
                MessageBox.Show("Không có dữ liệu cho ngày cuối cùng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var chartData = selectedRows.Select(r => new DevicePerformance
            {
                Machine_Name = r.Machine_Name,
                DailyPerformance = new Dictionary<string, double>
                {
                    { lastDate, r.DailyPerformance.ContainsKey(lastDate) ? r.DailyPerformance[lastDate] : 0 }
                }
            }).ToList();

            var detail = new ChartView(chartData);
            detail.ShowDialog();

        }


    }
}
