using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        public List<string> Dates { get; set; } = new List<string>(); // Danh sách ngày để tạo cột

        public event Action ImportCompleted;

        private ICommand clickButtonImportCommand;
        public ICommand ImportCommand => clickButtonImportCommand ?? (clickButtonImportCommand = new RelayCommand(ClickImportButton));

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

        public PerformanceViewModel()
		{
            PerFormanceData = new ObservableCollection<DevicePerformance>();
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

					//PerFormanceData.InsertToDatabase(dataList, UserName);
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
