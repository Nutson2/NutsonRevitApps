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

namespace MEPGadgets.MEPSystemFilters.View
{
    /// <summary>
    /// Логика взаимодействия для ViewSelectSystem.xaml
    /// </summary>
    public partial class ViewSelectSystem : Window
    {
        private readonly VMMEPSystemFilters vm;
        public ViewSelectSystem(VMMEPSystemFilters VM)
        {
            InitializeComponent();
            vm = VM;
            DataContext= vm;
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            vm.AddSelectedSystem();
        }

        private void ButCreateView_Click(object sender, RoutedEventArgs e)
        {
            vm.CreateView();
            Close();
        }

        private void ButAddTask_Click(object sender, RoutedEventArgs e)
        {
            vm.CreateTaskForFilter();

        }
    }
}
