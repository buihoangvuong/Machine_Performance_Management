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
using System.Windows.Shapes;

namespace Machine_Performance_Management.Chart
{
	/// <summary>
	/// Interaction logic for ChartView.xaml
	/// </summary>
	public partial class ChartView : Window
	{
		protected readonly ChartViewModel viewModel; //= new ChartViewModel();
		public ChartView(List<DevicePerformance> test)
		{
			InitializeComponent();

			viewModel = new ChartViewModel(test);
			DataContext = viewModel;
		}
	}
}
