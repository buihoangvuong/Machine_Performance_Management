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
        public PerformanceViewModel(string fullname)
        {
            DateTime serverTime = (DateTime)performanceModel.GetServerTime();
            DateFrom = serverTime;
            DateTo = serverTime;
            Fullname = fullname;
            PerFormanceData = new ObservableCollection<DevicePerformance>();
            LoadFactoryItems();
            LoadMachineTypeItems();
            LoadData();
            LoadMonths();
        }

        public List<string> Months { get; set; }

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

        private List<DevicePerformance1> DataGrid;

        public event Action ImportCompleted;
		#endregion

		private ICommand clickButtonImportCommand;
        public ICommand ImportCommand => clickButtonImportCommand ?? (clickButtonImportCommand = new RelayCommand(ClickImportButton));
        public ICommand _clickSearchCommand;
        public ICommand SearchCommand => _clickSearchCommand ?? (_clickSearchCommand = new RelayCommand(SearchMachine));

        private void SearchMachine()
        {
            var data = performanceModel.LoadPerformanceMachineList(
                SelectedFactoryItemDataManagement,
                SelectedMachineTypeItem, DateFrom, DateTo
            );
            PerFormanceData = new ObservableCollection<DevicePerformance>(data);
            DateHeaders = data
                .SelectMany(d => d.DailyPerformance.Keys)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }
        public void LoadData()
        {
            var data = performanceModel.LoadPerformanceMachineList(SelectedFactoryItemDataManagement, SelectedMachineTypeItem, DateFrom, DateTo);
            PerFormanceData = new ObservableCollection<DevicePerformance>(data);
            DateHeaders = data
           .SelectMany(d => d.DailyPerformance.Keys)
           .Distinct()
           .OrderBy(d => d)
           .ToList();
            ImportCompleted?.Invoke();
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
            var list = performanceModel.GetMachineTypeList();

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
                    var dataList = performanceModel.ReadExcelData(filePath);

                    if (dataList == null || dataList.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu để import", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var insertList = new List<DevicePerformance1>();
                    foreach (var d in dataList)
                    {
                        foreach (var date in d.DailyPerformance.Keys)
                        {
                            insertList.Add(new DevicePerformance1
                            {
                                Date = date,
                                Factory = d.Factory,
                                Item = d.Item,
                                Machine_Name = d.Machine_Name,

                                DailyPerformance = d.DailyPerformance.ContainsKey(date) ? d.DailyPerformance[date] : 0,
                                Performance_Target = d.Performance_Target.ContainsKey(date) ? d.Performance_Target[date] : 0,
                                Performance_Completed = d.Performance_Completed.ContainsKey(date) ? d.Performance_Completed[date] : 0,
                                Reason = d.Reason.ContainsKey(date) ? d.Reason[date] : null
                            });
                        }
                    }

					DataGrid = new List<DevicePerformance1>();
					if (insertList.Any())
					{
						DataGrid.AddRange(insertList);
					}

					performanceModel.InsertToDatabase(insertList, Fullname);
                    PerFormanceData.Clear();
                    foreach (var item in dataList)
                    {
                        PerFormanceData.Add(item);
                    }
                    DateHeaders = dataList
                    .SelectMany(d => d.DailyPerformance.Keys)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();
                    ImportCompleted?.Invoke();

                    MessageBox.Show("Import thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Import thất bại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void LoadMonths()
        {
            Months = Enumerable.Range(1, 12)
                .Select(i => i.ToString("D2"))
                .ToList();
            Months.Insert(0, "All");
            SelectedMonth = Months[0];
            OnPropertyChanged(nameof(Months));
        }
        private string _selectedMonth;
        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    OnPropertyChanged(nameof(SelectedMonth));
                    LoadData();
                }
            }
        }


        //public void LoadData(List<DevicePerformance> data)
        //{
        //    PerFormanceData.Clear();
        //    Dates.Clear();

        //    foreach (var item in data)
        //    {
        //        foreach (var day in item.DailyPerformance.Keys)
        //        {
        //            if (!Dates.Contains(day))
        //                Dates.Add(day);
        //        }
        //        PerFormanceData.Add(item);
        //    }
        //}
    }
}
