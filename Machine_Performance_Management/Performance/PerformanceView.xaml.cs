using Machine_Performance_Management.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace Machine_Performance_Management.Performance
{
	/// <summary>
	/// Interaction logic for PerformanceView.xaml
	/// </summary>
	public partial class PerformanceView : UserControl
	{
		protected readonly PerformanceViewModel viewModel;

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
        private void AddDynamicDateColumns1(DataGrid dataGrid, List<string> dates)
        {
            if (dates == null || dates.Count == 0)
                return;

            dataGrid.Columns.Clear();

            var centerTextStyle = new Style(typeof(TextBlock));
            centerTextStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            centerTextStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));


            dataGrid.Columns.Add(new DataGridTextColumn { Header = "NO", Binding = new Binding("NO"), Width = 40, ElementStyle = centerTextStyle});
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Factory", Binding = new Binding("Factory"), Width = 70, ElementStyle = centerTextStyle });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Machine Type", Binding = new Binding("Item"), Width = 95, ElementStyle = centerTextStyle });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Machine Name", Binding = new Binding("Machine_Name"), Width = 110, ElementStyle = centerTextStyle });

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

                if (date == dates.Last())
                {
                    col.CellStyle = new Style(typeof(DataGridCell))
                    {
                        Setters =
                        {
                            new Setter(DataGridCell.BackgroundProperty, Brushes.LightYellow),
                            new Setter(DataGridCell.FontWeightProperty, FontWeights.Bold),
                            new Setter(DataGridCell.ForegroundProperty, Brushes.DarkRed)
                        }
                    };
                }
                dataGrid.Columns.Add(col);
            }

            dataGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Average",
                Binding = new Binding("AveragePerformance")
                {
                    StringFormat = "{0:0.##}%"  // 🔹 Thêm %
                },
                Width = 70,
                ElementStyle = centerTextStyle
            });
        }

        private void AddDynamicDateColumns(DataGrid dataGrid, List<string> dates)
        {
            if (dates == null || dates.Count == 0) return;

            dataGrid.Columns.Clear();

            // cột chọn dòng (checkbox) ở vị trí đầu
            dataGrid.Columns.Add(BuildSelectColumn());

            var centerTextStyle = new Style(typeof(TextBlock));
            centerTextStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            centerTextStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "NO", Binding = new Binding("NO"), Width = 40, ElementStyle = centerTextStyle });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Factory", Binding = new Binding("Factory"), Width = 70, ElementStyle = centerTextStyle });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Machine Type", Binding = new Binding("Item"), Width = 95, ElementStyle = centerTextStyle });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Machine Name", Binding = new Binding("Machine_Name"), Width = 110, ElementStyle = centerTextStyle });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Average",
                Binding = new Binding("AveragePerformance") { StringFormat = "{0:0.##}%" },
                Width = 70,
                ElementStyle = centerTextStyle
            });

            foreach (var date in dates)
            {
                var col = new DataGridTextColumn
                {
                    Header = date,
                    Binding = new Binding($"DailyPerformance[{date}]") { StringFormat = "{0:0.##}%" },
                    Width = 60,
                    ElementStyle = centerTextStyle
                };

                if (date == dates.Last())
                {
                    col.CellStyle = new Style(typeof(DataGridCell))
                    {
                        Setters =
                {
                    new Setter(DataGridCell.BackgroundProperty, Brushes.LightYellow),
                    new Setter(DataGridCell.FontWeightProperty, FontWeights.Bold),
                    new Setter(DataGridCell.ForegroundProperty, Brushes.DarkRed)
                }
                    };
                }

                dataGrid.Columns.Add(col);
            }

            // sau khi build cột xong, theo dõi thay đổi để cập nhật trạng thái "Select All"
            WireSelectionMonitoring();
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

        private CheckBox _selectAllCheckBox;

        private DataGridTemplateColumn BuildSelectColumn()
        {
            _selectAllCheckBox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsThreeState = true
            };
            _selectAllCheckBox.Click += SelectAllCheckBox_Click;

            var f = new FrameworkElementFactory(typeof(CheckBox));
            f.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            f.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            f.SetBinding(ToggleButton.IsCheckedProperty,
                new Binding("IsSelected")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

            var cellTemplate = new DataTemplate { VisualTree = f };

            return new DataGridTemplateColumn
            {
                Header = _selectAllCheckBox,
                CellTemplate = cellTemplate,
                Width = 40
            };
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel?.PerFormanceData == null) return;

            // Lấy trạng thái của checkbox header
            var isChecked = _selectAllCheckBox.IsChecked;

            if (isChecked == true)
            {
                // Chọn tất cả
                foreach (var item in viewModel.PerFormanceData)
                    item.IsSelected = true;
            }
            else if (isChecked == false)
            {
                // Bỏ chọn tất cả
                foreach (var item in viewModel.PerFormanceData)
                    item.IsSelected = false;
            }

            // Sau khi đổi -> cập nhật lại trạng thái header (đảm bảo đồng bộ)
            UpdateSelectAllState();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DevicePerformance.IsSelected))
            {
                // khi tick/untick row con thì cập nhật header
                UpdateSelectAllState();
            }
        }

        private void WireSelectionMonitoring()
        {
            if (viewModel?.PerFormanceData == null) return;

            foreach (var item in viewModel.PerFormanceData)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                item.PropertyChanged += Item_PropertyChanged;
            }
            UpdateSelectAllState();
        }



        private void UpdateSelectAllState()
        {
            if (_selectAllCheckBox == null || viewModel?.PerFormanceData == null || viewModel.PerFormanceData.Count == 0)
                return;

            int total = viewModel.PerFormanceData.Count;
            int checkedCount = viewModel.PerFormanceData.Count(x => x.IsSelected);

            if (checkedCount == 0)
                _selectAllCheckBox.IsChecked = false;
            else if (checkedCount == total)
                _selectAllCheckBox.IsChecked = true;
            else
                _selectAllCheckBox.IsChecked = null; // indeterminate
        }
    }
}
