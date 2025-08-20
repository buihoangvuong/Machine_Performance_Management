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

        private List<DevicePerformance1> DataGrid;

        public event Action ImportCompleted;
		#endregion

		private ICommand clickButtonImportCommand;
        public ICommand ImportCommand => clickButtonImportCommand ?? (clickButtonImportCommand = new RelayCommand(ClickImportButton));


        public PerformanceViewModel(string fullname)
        {
            Fullname = fullname;
            PerFormanceData = new ObservableCollection<DevicePerformance>();
            LoadFactoryItems();
            LoadData();
        }

        public void LoadData()
        {
            var data = performanceModel.LoadPerformanceMachineList(SelectedFactoryItemDataManagement);
            PerFormanceData = new ObservableCollection<DevicePerformance>(data);
            DateHeaders = data
           .SelectMany(d => d.DailyPerformance.Keys)
           .Distinct()
           .OrderBy(d => d)
           .ToList();
            ImportCompleted?.Invoke();

            //DataGrid = new List<DevicePerformance1>();
            //foreach (var d in data)
            //{
            //    foreach (var date in d.DailyPerformance.Keys)
            //    {
            //        DataGrid.Add(new DevicePerformance1
            //        {
            //            Date = date,
            //            Factory = d.Factory,
            //            Item = d.Item,
            //            Machine_Name = d.Machine_Name,
            //            //Performance_ST = d.Performance_ST.ContainsKey(date) ? d.Performance_ST[date] : 0,
            //            DailyPerformance = d.DailyPerformance.ContainsKey(date) ? d.DailyPerformance[date] : 0,
            //            Performance_Target = d.Performance_Target.ContainsKey(date) ? d.Performance_Target[date] : 0,
            //            Performance_Completed = d.Performance_Completed.ContainsKey(date) ? d.Performance_Completed[date] : 0,
            //            Reason = d.Reason.ContainsKey(date) ? d.Reason[date] : null
            //        });
            //    }
            //}

            //DataGrid = new List<DevicePerformance1>();
            //if (dataList.Any())
            //{
            //    DataGrid.AddRange(dataList);
            //}
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

    }
}
