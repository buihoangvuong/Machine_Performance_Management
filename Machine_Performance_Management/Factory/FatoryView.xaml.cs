using Machine_Performance_Management.User;
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

namespace Machine_Performance_Management.Factory
{
    /// <summary>
    /// Interaction logic for FatoryView.xaml
    /// </summary>
    public partial class FatoryView : UserControl
    {
        private readonly FactoryViewModel factoryViewModel;
        public FatoryView(string fullname)
        {
            InitializeComponent();
            factoryViewModel = new FactoryViewModel(fullname)
            {
                Fullname = fullname
            };
            DataContext = factoryViewModel;
        }
    }
}
