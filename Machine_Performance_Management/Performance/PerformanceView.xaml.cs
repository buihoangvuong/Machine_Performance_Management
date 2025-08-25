using LiveCharts;
using LiveCharts.Wpf;
using Machine_Performance_Management.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;


namespace Machine_Performance_Management.Performance
{
    /// <summary>
    /// Interaction logic for PerformanceView.xaml
    /// </summary>
    /// 

    // Converter để so sánh giá trị
   

    // Thêm các using statements cần thiết:
    // using System.Globalization;
    // using System.Windows.Data;

    // Converter để xử lý màu sắc dựa trên giá trị performance
    public class PerformanceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;

            var condition = parameter?.ToString() ?? "";

            if (double.TryParse(value.ToString(), out double numericValue))
            {
                switch (condition)
                {
                    case ">=50":
                        return numericValue >= 50;
                    case "<50":
                        return numericValue < 50;
                    case ">85":
                        return numericValue > 85;
                    default:
                        return false;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class PerformanceView : UserControl
	{
		protected readonly PerformanceViewModel viewModel;

        public SeriesCollection SmtSeries { get; set; } = new SeriesCollection();

        public PerformanceView(string fullname)
		{
			InitializeComponent();
            viewModel = new PerformanceViewModel(fullname);
            DataContext = viewModel;
            viewModel.ImportCompleted += OnImportCompleted;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.Loaded += PerformanceView_Loaded;
        }
        private void PerformanceView_Loaded(object sender, RoutedEventArgs e)
        {
            AddDynamicDateColumns(MyDataGrid, viewModel.DateHeaders);
        }
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(viewModel.DateHeaders))
            {
                AddDynamicDateColumns(MyDataGrid, viewModel.DateHeaders);
            }
        }






        private void AddDynamicDateColumns(DataGrid dataGrid, List<string> dates)
        {
            if (dates == null || dates.Count == 0)
                return;

            dataGrid.Columns.Clear();

            var centerTextStyle = new Style(typeof(TextBlock));
            centerTextStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            centerTextStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            centerTextStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "NO", Binding = new Binding("NO"), Width = 40, ElementStyle = centerTextStyle });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Factory", Binding = new Binding("Factory"), Width = 70, ElementStyle = centerTextStyle });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Machine Type", Binding = new Binding("Item"), Width = 95, ElementStyle = centerTextStyle });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Machine Name", Binding = new Binding("Machine_Name"), Width = 120, ElementStyle = centerTextStyle });

            foreach (var date in dates)
            {
                var col = new DataGridTextColumn
                {
                    Header = date,
                    Binding = new Binding($"DailyPerformance[{date}]")
                    {
                        StringFormat = "{0:0.##}%"
                    },
                    Width = 60,
                    ElementStyle = centerTextStyle
                };

                var cellTemplate = new DataTemplate();

                // Border
                FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
                border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
                border.SetValue(Border.PaddingProperty, new Thickness(4, 2, 4, 2));
                border.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);

                // Text
                FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
                text.SetBinding(TextBlock.TextProperty, new Binding($"DailyPerformance[{date}]") { StringFormat = "{0:0.##}%" });
                text.SetValue(TextBlock.ForegroundProperty, Brushes.White);
                text.SetValue(TextBlock.FontSizeProperty, 12.0);
                text.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
                text.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);


                // Tạo CellStyle với điều kiện màu sắc
                var cellStyle = new Style(typeof(DataGridCell));
                cellStyle.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(0)));

                // Trigger cho MouseOver 
                var mouseOverTrigger = new Trigger();
                mouseOverTrigger.Property = DataGridCell.IsMouseOverProperty;
                mouseOverTrigger.Value = true;
                mouseOverTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty,
                    new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x3E))));

                // Trigger cho Selected 
                var selectedTrigger = new Trigger();
                selectedTrigger.Property = DataGridCell.IsSelectedProperty;
                selectedTrigger.Value = true;
                selectedTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty,
                    new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x3E))));

                // DataTrigger cho giá trị >= 50 (màu vàng)
                var colorTrigger1 = new DataTrigger();
                colorTrigger1.Binding = new Binding($"DailyPerformance[{date}]")
                {
                    Converter = new PerformanceColorConverter(),
                    ConverterParameter = ">=50"
                };
                colorTrigger1.Value = true;
                colorTrigger1.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.Gold));

                // DataTrigger cho giá trị < 50 (màu đỏ)
                var colorTrigger2 = new DataTrigger();
                colorTrigger2.Binding = new Binding($"DailyPerformance[{date}]")
                {
                    Converter = new PerformanceColorConverter(),
                    ConverterParameter = "<50"
                };
                colorTrigger2.Value = true;
                colorTrigger2.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.Red));

                // DataTrigger 
                var colorTrigger3 = new DataTrigger();
                colorTrigger3.Binding = new Binding($"DailyPerformance[{date}]")
                {
                    Converter = new PerformanceColorConverter(),
                    ConverterParameter = ">85"
                };

                colorTrigger3.Value = true;
                colorTrigger3.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.Green));

                // Thêm triggers theo thứ tự: background trước, foreground sau
                cellStyle.Triggers.Add(mouseOverTrigger);
                cellStyle.Triggers.Add(selectedTrigger);
                cellStyle.Triggers.Add(colorTrigger1);
                cellStyle.Triggers.Add(colorTrigger2);
                cellStyle.Triggers.Add(colorTrigger3);

                // Nếu là cột cuối cùng, thêm background màu vàng nhạt và font đậm
                //if (date == dates.Last())
                //{
                //    // Override background cho cột cuối
                //    cellStyle.Setters.RemoveAt(cellStyle.Setters.Count - 1); // Remove transparent background
                //    cellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightYellow));
                //    cellStyle.Setters.Add(new Setter(DataGridCell.FontWeightProperty, FontWeights.Bold));

                //    // Update triggers cho cột cuối để giữ màu vàng nhạt khi hover/select
                //    mouseOverTrigger.Setters.Clear();
                //    mouseOverTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightYellow));

                //    selectedTrigger.Setters.Clear();
                //    selectedTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightYellow));
                //}

                col.CellStyle = cellStyle;
                dataGrid.Columns.Add(col);
            }

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Average",
                Binding = new Binding("AveragePerformance")
                {
                    StringFormat = "{0:0.##}%"
                },
                Width = 80,
                ElementStyle = centerTextStyle
            });
        }




        private void OnImportCompleted()
        {
            AddDynamicDateColumns(MyDataGrid, viewModel.DateHeaders);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			var grid = sender as DataGrid;
			if (grid == null) return;

			// tìm row + cell
			var dep = (DependencyObject)e.OriginalSource;
            DataGridRow row = null;
            DataGridCell cell = null;

            while (dep != null)
            {
                if (dep is DataGridCell c) cell = c;
                if (dep is DataGridRow r) { row = r; break; }
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (row == null || cell == null)
            {
                DetailPopup.IsOpen = false;
                return;
            }

            if (cell.DataContext is DevicePerformance device)
            {
                string header = grid.CurrentCell.Column.Header?.ToString();
                if (string.IsNullOrEmpty(header)) return;

                ShowPopup(device, header);
            }
        }

        /// <summary>
        /// Hiển thị popup với donut + info
        /// </summary>
        /// 
        private void ShowPopup(DevicePerformance device, string header)
        {
            if (device.DailyPerformance.TryGetValue(header, out double st) &&
                device.Performance_Target.TryGetValue(header, out double target) &&
                device.Performance_Completed.TryGetValue(header, out double completed) &&
                device.Reason.TryGetValue(header, out string reason))
            {
                // Fill thông tin
                PopupModel.Text = device.Machine_Name;
                PopupST.Text = st.ToString("0.##");
                PopupTarget.Text = target.ToString("0.##");
                PopupCompleted.Text = completed.ToString("0.##");
                PopupReason.Text = reason;

                // % completed
                double percent = (target > 0) ? (completed / target * 85) : 0;
                DonutPercent.Text = $"{percent:0}%";

                // Chart series
                MyPieChart.Series = new SeriesCollection
        {
            new PieSeries
            {
                Values = new ChartValues<double> { percent },
                Fill = Brushes.Aqua,
                DataLabels = false
            },
            new PieSeries
            {
                Values = new ChartValues<double> { 100 - percent },
                Fill = Brushes.Gray,
                DataLabels = false
            }
        };

                DetailPopup.IsOpen = true;
            }
            else
            {
                DetailPopup.IsOpen = false;
            }
        }
    }

    public partial class PerformanceView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
