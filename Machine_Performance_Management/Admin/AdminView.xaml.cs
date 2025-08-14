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

namespace Machine_Performance_Management.Admin
{
    public partial class AdminView : UserControl
    {
        private readonly AdminViewModel adminViewModel;
        public AdminView(string fullname)
        {
            InitializeComponent();
            adminViewModel = new AdminViewModel(fullname)
            {
                Fullname = fullname
            };
            DataContext = adminViewModel;
        }

        private void OperationDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
