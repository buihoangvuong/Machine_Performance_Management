using Machine_Performance_Management.Chart;
using Machine_Performance_Management.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        private void AddDynamicDateColumns(DataGrid dataGrid, List<string> dates)
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
            //var machineColumn = new DataGridTemplateColumn
            //{
            //    Header = "Machine Name",
            //    Width = 120
            //};

            // Tạo TextBlock trong CellTemplate
            //var textFactory = new FrameworkElementFactory(typeof(TextBlock));
            //textFactory.SetBinding(TextBlock.TextProperty, new Binding("Machine_Name"));
            //textFactory.SetValue(TextBlock.ForegroundProperty, Brushes.Blue);
            //textFactory.SetValue(TextBlock.CursorProperty, Cursors.Hand);
            //textFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            //textFactory.SetValue(TextBlock.TextDecorationsProperty, TextDecorations.Underline);

            // Gán event click bằng style (hoặc attach command sau)
            //textFactory.AddHandler(TextBlock.MouseLeftButtonUpEvent, new MouseButtonEventHandler(MachineNameClickHandler));

            //machineColumn.CellTemplate = new DataTemplate { VisualTree = textFactory };
            //dataGrid.Columns.Add(machineColumn);


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

        private void MachineNameClickHandler(object sender, MouseButtonEventArgs e)
        {
            //if (sender is TextBlock tb && tb.DataContext is DevicePerformance machine)
            //{
            //    DevicePerformance1 test = new DevicePerformance1(); // hoặc lấy từ data hiện có
            //    var detail = new ChartView(test);
            //    //var detail = new ChartView();
            //    detail.ShowDialog();
            //}
            //if (data != null && data.Any())
            //{
            //    var detail = new ChartView(data); // ✅ truyền cả danh sách
            //    detail.ShowDialog();
            //}
        }

        private void OnImportCompleted()
        {
            AddDynamicDateColumns(MyDataGrid, viewModel.DateHeaders);
        }

        private void DataGrid_MouseDoubleClick1(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null) return;

            // Kiểm tra có click vào row không
            var dep = (DependencyObject)e.OriginalSource;
            DataGridRow row = null;
            DataGridCell cell = null;

            while (dep != null)
            {
                if (dep is DataGridCell)
                    cell = dep as DataGridCell;
                if (dep is DataGridRow)
                {
                    row = dep as DataGridRow;
                    break;
                }
                dep = VisualTreeHelper.GetParent(dep);
            }

            // Nếu không click vào row hoặc không click vào cell nào -> tắt popup
            if (row == null || cell == null)
            {
                DetailPopup.IsOpen = false;
                return;
            }

            if (grid.CurrentCell == null) return;

            var column = grid.CurrentCell.Column as DataGridTextColumn;
            if (column == null)
            {
                DetailPopup.IsOpen = false;
                return;
            }

            string dateHeader = column.Header?.ToString();
            if (string.IsNullOrEmpty(dateHeader) ||
                dateHeader == "NO" ||
                dateHeader == "Factory" ||
                dateHeader == "Machine Name" ||
                dateHeader == "Average")
            {
                DetailPopup.IsOpen = false;
                return;
            }

            if (cell != null)
            {
                // lấy DevicePerformance từ cell (DataContext của row)
                if (cell.DataContext is DevicePerformance device)
                {
                    if (device.Performance_ST.ContainsKey(dateHeader) &&
                        device.Performance_Target.ContainsKey(dateHeader) &&
                        device.Performance_Completed.ContainsKey(dateHeader) &&
                        device.Reason.ContainsKey(dateHeader))
                    {
                        string model = device.Machine_Name;
                        double st = device.Performance_ST[dateHeader];
                        double target = device.Performance_Target[dateHeader];
                        double completed = device.Performance_Completed[dateHeader];
                        string reason = device.Reason[dateHeader];

                        // Gán dữ liệu cho popup với nhiều màu
                        PopupModel.Inlines.Clear();
                        PopupModel.Inlines.Add(new Run("🖥️ ") { Foreground = Brushes.DodgerBlue });
                        PopupModel.Inlines.Add(new Run("Model: ") { Foreground = Brushes.Black });
                        PopupModel.Inlines.Add(new Run(model) { Foreground = Brushes.Red });

                        PopupST.Inlines.Clear();
                        PopupST.Inlines.Add(new Run("🎯 ") { Foreground = Brushes.DodgerBlue });
                        PopupST.Inlines.Add(new Run("ST: ") { Foreground = Brushes.Black });
                        PopupST.Inlines.Add(new Run(st.ToString()) { Foreground = Brushes.Red });

                        PopupCompleted.Inlines.Clear();
                        PopupCompleted.Inlines.Add(new Run("✅ ") { Foreground = Brushes.DodgerBlue });
                        PopupCompleted.Inlines.Add(new Run("Completed: ") { Foreground = Brushes.Black });
                        PopupCompleted.Inlines.Add(new Run($"{completed} / {target}") { Foreground = Brushes.Red });

                        PopupReason.Inlines.Clear();
                        PopupReason.Inlines.Add(new Run("🕒 ") { Foreground = Brushes.DodgerBlue });
                        PopupReason.Inlines.Add(new Run("Reason: ") { Foreground = Brushes.Black });
                        PopupReason.Inlines.Add(new Run(reason) { Foreground = Brushes.Red });

                        DetailPopup.IsOpen = true;
                        return;
                    }
                }
            }

            DetailPopup.IsOpen = false;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null) return;

            // Nếu chưa chọn cell thì return
            if (grid.CurrentCell == null || grid.CurrentItem == null)
            {
                DetailPopup.IsOpen = false;
                return;
            }

            // Row data (object được bind)
            var device = grid.CurrentItem as DevicePerformance;
            if (device == null)
            {
                DetailPopup.IsOpen = false;
                return;
            }

            // Column được double click
            var column = grid.CurrentCell.Column as DataGridTextColumn;
            if (column == null)
            {
                DetailPopup.IsOpen = false;
                return;
            }

            string header = column.Header?.ToString();
            if (string.IsNullOrEmpty(header) ||
                header == "NO" ||
                header == "Factory" ||
                header == "Machine Name" ||
                header == "Average")
            {
                DetailPopup.IsOpen = false;
                return;
            }

            // Kiểm tra dictionary có key = header không
            if (device.Performance_ST.TryGetValue(header, out double st) &&
                device.Performance_Target.TryGetValue(header, out double target) &&
                device.Performance_Completed.TryGetValue(header, out double completed) &&
                device.Reason.TryGetValue(header, out string reason))
            {
                // Hiển thị dữ liệu đẹp hơn
                PopupModel.Inlines.Clear();
                PopupModel.Inlines.Add(new Run("🖥️ ") { Foreground = Brushes.DodgerBlue });
                PopupModel.Inlines.Add(new Run("Model: ") { Foreground = Brushes.Black });
                PopupModel.Inlines.Add(new Run(device.Machine_Name) { Foreground = Brushes.Red });

                PopupST.Inlines.Clear();
                PopupST.Inlines.Add(new Run("🎯 ") { Foreground = Brushes.DodgerBlue });
                PopupST.Inlines.Add(new Run("ST: ") { Foreground = Brushes.Black });
                PopupST.Inlines.Add(new Run(st.ToString()) { Foreground = Brushes.Red });

                PopupCompleted.Inlines.Clear();
                PopupCompleted.Inlines.Add(new Run("✅ ") { Foreground = Brushes.DodgerBlue });
                PopupCompleted.Inlines.Add(new Run("Completed: ") { Foreground = Brushes.Black });
                PopupCompleted.Inlines.Add(new Run($"{completed} / {target}") { Foreground = Brushes.Red });

                PopupReason.Inlines.Clear();
                PopupReason.Inlines.Add(new Run("🕒 ") { Foreground = Brushes.DodgerBlue });
                PopupReason.Inlines.Add(new Run("Reason: ") { Foreground = Brushes.Black });
                PopupReason.Inlines.Add(new Run(reason) { Foreground = Brushes.Red });

                DetailPopup.PlacementTarget = grid; // gắn popup gần grid
                DetailPopup.IsOpen = true;
            }
            else
            {
                DetailPopup.IsOpen = false;
            }
        }
    }
}
