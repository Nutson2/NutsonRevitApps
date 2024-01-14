using CopyParametersGadgets.WriteCalculation.Model;
using CopyParametersGadgets.WriteCalculation.ViewModel;
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

namespace CopyParametersGadgets.WriteCalculation.View
{
    /// <summary>
    /// Логика взаимодействия для ViewCalculation.xaml
    /// </summary>
    public partial class ViewCalculation : Window
    {
        private readonly VMCalculation vm;

        public ViewCalculation(VMCalculation VM)
        {
            InitializeComponent();
            vm= VM;
            DataContext= vm;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            vm.Apply();
            Close();
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            vm.AddCalculationModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var model=(CalculationModel)((Button)sender).DataContext;
            vm.CalculationModels.Remove(model);
        }
    }
}
