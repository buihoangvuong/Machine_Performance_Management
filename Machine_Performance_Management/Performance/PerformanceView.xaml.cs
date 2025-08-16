using Machine_Performance_Management.Common;
using System;
using System.Collections.Generic;
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
		protected readonly PerformanceViewModel viewModel = new PerformanceViewModel();
		public PerformanceView(string fullname)
		{
			InitializeComponent();
            viewModel = new PerformanceViewModel(fullname);
            DataContext = viewModel;
            viewModel.ImportCompleted += OnImportCompleted;
        }

        private void AddDynamicDateColumns(DataGrid dataGrid, List<string> dates)
        {
            if (dates == null || dates.Count == 0)
                return;

            dataGrid.Columns.Clear();

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "NO", Binding = new Binding("NO"), Width = 40 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Factory", Binding = new Binding("Factory"), Width = 80 });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Machine Name", Binding = new Binding("Machine_Name"), Width = 150 });

            foreach (var date in dates)
            {
                var col = new DataGridTextColumn
                {
                    Header = date,
                    Binding = new Binding($"DailyPerformance[{date}]") 
                    { 
                        StringFormat = "{0:0.##}%" 
                    },
                    Width = 80
                };
                dataGrid.Columns.Add(col);
            }

            dataGrid.Columns.Add(new DataGridTextColumn 
            { 
                Header = "Average",
                Binding = new Binding("AveragePerformance")
                {
                    StringFormat = "{0:0.##}%"  // 🔹 Thêm %
                },
                Width = 80 
            });
        }

        private void OnImportCompleted()
        {
            AddDynamicDateColumns(MyDataGrid, viewModel.DateHeaders);
        }

		private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
            var cell = e.OriginalSource as FrameworkElement;
            if (cell == null)
                return;

            // Lấy DataContext của dòng hiện tại
            var rowData = (cell.DataContext as DevicePerformance);
            if (rowData == null)
                return;

            // Lấy cột được click
            var column = (cell.Parent as DataGridCell)?.Column as DataGridBoundColumn;
            if (column == null)
                return;

            // Header chính là ngày
            string dateHeader = column.Header as string;

            // Kiểm tra xem đây có phải cột ngày không
            if (rowData.DailyPerformance.ContainsKey(dateHeader))
            {
                double target = rowData.Performance_Target.ContainsKey(dateHeader)
                    ? rowData.Performance_Target[dateHeader]
                    : 0;

                double completed = rowData.Performance_Completed.ContainsKey(dateHeader)
                    ? rowData.Performance_Completed[dateHeader]
                    : 0;

                MessageBox.Show(
                    $"Ngày: {dateHeader}\n" +
                    $"Capa/일 (Target): {target}\n" +
                    $"생산량 (Completed): {completed}",
                    "Chi tiết hiệu suất",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.CurrentCell != null)
            {
                var column = grid.CurrentCell.Column as DataGridTextColumn;
                if (column == null) return;

                // Lấy tiêu đề cột (chính là ngày)
                string dateHeader = column.Header?.ToString();
                if (string.IsNullOrEmpty(dateHeader) || dateHeader == "NO" || dateHeader == "Factory" || dateHeader == "Machine Name" || dateHeader == "Average")
                    return;

                // Lấy đối tượng DevicePerformance đang chọn
                if (grid.SelectedItem is DevicePerformance device)
                {
                    if (device.Performance_Target.ContainsKey(dateHeader) && device.Performance_Completed.ContainsKey(dateHeader) && device.Reason.ContainsKey(dateHeader))
                    {
                        double target = device.Performance_Target[dateHeader];
                        double completed = device.Performance_Completed[dateHeader];
                        string reason = device.Reason[dateHeader];

                        MessageBox.Show(
                            $"📅 Date: {dateHeader}\n🎯 Target: {target}\n✅ Completed: {completed}\n🕒 Reason: {reason}",
                            "Thông tin chi tiết",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                }
            }
        }
    }
}
