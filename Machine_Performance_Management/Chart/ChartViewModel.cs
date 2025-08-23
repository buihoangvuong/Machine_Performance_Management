using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Machine_Performance_Management.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Machine_Performance_Management.Chart
{
	public class ChartViewModel : ObservableObject
	{
		//private ChartModel chartModel = new ChartModel();

		private List<DevicePerformance> data;

		public SeriesCollection Series { get; set; }
		public string[] Labels { get; set; }
		public Func<double, string> YFormatter { get; set; }

		// CheckBox
		private bool _isByMachineType;
		public bool IsByMachineType
		{
			get => _isByMachineType;
			set
			{
                if (_isByMachineType != value)
                {
                    _isByMachineType = value;
                    OnPropertyChanged(nameof(IsByMachineType));

                    if (_isByMachineType)
                    {
                        IsByMachine = false;
                    }

                    RefreshChart();
                }
            }
		}

		// CheckBox
		private bool _isByMachine;
		public bool IsByMachine
		{
			get => _isByMachine;
			set
			{
				if (_isByMachine != value)
				{
					_isByMachine = value;
					OnPropertyChanged(nameof(IsByMachine));

                    if (_isByMachine)
                    {
                        IsByMachineType = false;
                    }

                    RefreshChart();
                }
			}
		}
		private DateTime? _selectedDate;
		public DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				if (_selectedDate != value)
				{
					_selectedDate = value;
					OnPropertyChanged(nameof(SelectedDate));
					RefreshChart();
				}
			}
		}
		private ObservableCollection<DateTime> _availableDates;
		public ObservableCollection<DateTime> AvailableDates
		{
			get => _availableDates;
			set
			{
				_availableDates = value;
				OnPropertyChanged(nameof(AvailableDates));
			}
		}


		public ChartViewModel(List<DevicePerformance> test)
		{
			data = test ?? new List<DevicePerformance>();

			//AvailableDates = new ObservableCollection<DateTime>(
			//	data.Select(d => DateTime.ParseExact(d.Date, "dd.MM", null))
			//		.Distinct()
			//		.OrderBy(d => d)
			//);

			LoadAveragePerformance();
		}

        // ========== Core Functions ==========
        private void RefreshChart()
        {
            if (IsByMachineType)
            {
                LoadPerformanceByItem(SelectedDate);
            }
            else if (IsByMachine)
            {
                LoadPerformanceByMachine(SelectedDate);
            }
            else
            {
                LoadAveragePerformance();
            }
        }

        private void LoadAveragePerformance()
        {
            //var grouped = data
                //.GroupBy(x => "Tất cả") // gom hết lại 1 cột
                //.Select(g => new
                //{
                //    Key = g.Key,
                //    AvgPerformance = g.Average(r => r.DailyPerformance)
                //})
                //.ToList();

            Series = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Hiệu suất trung bình (all)",
                //Values = new ChartValues<double>(grouped.Select(x => x.AvgPerformance))
            }
        };

            //Labels = grouped.Select(x => x.Key).ToArray();
            YFormatter = v => v.ToString("F2") + "%";

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(Labels));
        }

        private void LoadPerformanceByItem(DateTime? date = null)
        {
            //var filtered = date.HasValue
            //    ? data.Where(x => x.Date == date.Value.ToString("dd.MM")) // Chuyển DateTime sang chuỗi "06.08"
            //    : data;
            //var filtered = !string.isnullorempty(date)
            //    ? data.where(x => x.date == date)
            //    : data;

            //var grouped = filtered
            //    .Where(x => !string.IsNullOrWhiteSpace(x.Item))
            //    .GroupBy(x => x.Item)
            //    .Select(g => new
            //    {
            //        Item = g.Key,
            //        AvgPerformance = g.Average(r => r.DailyPerformance)
            //    })
            //    .OrderBy(x => x.Item)
            //    .ToList();

            Series = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Theo loại máy",
            //Values = new ChartValues<double>(grouped.Select(x => x.AvgPerformance))
        }
    };

            //Labels = grouped.Select(x => x.Item).ToArray();
            //YFormatter = v => v.ToString("F2") + "%";

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(Labels));
        }

        private void LoadPerformanceByMachine(DateTime? date = null)
        {
            //var filtered = date.HasValue
            //? data.Where(x => x.Date == date.Value.ToString("dd.MM")) // Chuyển DateTime sang chuỗi "06.08"
            //: data;

            //var grouped = filtered
            //    .Where(x => !string.IsNullOrWhiteSpace(x.Machine_Name))
            //    .GroupBy(x => x.Machine_Name)
            //    .Select(g => new
            //    {
            //        Machine = g.Key,
            //        AvgPerformance = g.Average(r => r.DailyPerformance)
            //    })
            //    .OrderBy(x => x.Machine)
            //    .ToList();

            Series = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Theo máy",
            //Values = new ChartValues<double>(grouped.Select(x => x.AvgPerformance))
        }
    };

            //Labels = grouped.Select(x => x.Machine).ToArray();
            YFormatter = v => v.ToString("F2") + "%";

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(Labels));
        }
    }
}
