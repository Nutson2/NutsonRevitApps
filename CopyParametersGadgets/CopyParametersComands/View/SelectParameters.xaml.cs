using System.Windows;
using System.Windows.Controls;



namespace CopyParametersGadgets
{
    /// <summary>
    /// Логика взаимодействия для SelectParameters.xaml
    /// </summary>
    public partial class SelectParameters : Window
    {
        readonly DataCopyParameterVMBase dataCopyShared;
        public SelectParameters(DataCopyParameterVMBase DataCopyShared)
        {
            InitializeComponent();
            dataCopyShared = DataCopyShared;
            DataContext = dataCopyShared;
            lbSharedParameters.ItemsSource = dataCopyShared.ParamSet;
        }

        private void BtCopy_Click(object sender, RoutedEventArgs e)
        {
            dataCopyShared.CopyParameters(lbSharedParameters.SelectedItems);
            this.Close();
        }

        private void lbSharedParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel=lbSharedParameters.SelectedItems.Count;
            if(sel > 0)
            {
                btCopy.IsEnabled = true;
                btCopy.Content = $"Копировать значения в элементы групп ({sel})";
            }
            else { btCopy.IsEnabled = false; }
        }
    }
}
