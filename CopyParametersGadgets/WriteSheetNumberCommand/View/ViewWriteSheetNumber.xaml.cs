using Autodesk.Revit.DB;
using CopyParametersGadgets.Model;
using CopyParametersGadgets.VM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NRPUtils.Model;

namespace CopyParametersGadgets.WriteSheetNumberCommand.View
{
    /// <summary>
    /// Логика взаимодействия для ViewWriteSheetNumber.xaml
    /// </summary>
    public partial class ViewWriteSheetNumber : Window
    {
        private readonly VMWriteSheetNumber VM;
        public ViewWriteSheetNumber(VMWriteSheetNumber _VM)
        {
            VM = _VM;
            InitializeComponent();
            DataContext=VM;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            VM.WriteSheetNumberToElements();
            Close();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var but = (Button)sender;

            VM.MoveUpStringParts(but.DataContext);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var but = (Button)sender;

            VM.RemoveStringParts(but.DataContext);
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var but = (Button)sender;
            VM.MoveDownStringParts(but.DataContext);
        }

        private void TVAvailableParameters_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var node = (Node<ParametersModel>)TVAvailableParameters.SelectedItem;
            if (node != null) { VM.AddParameterStringParts(node.Item ); }
        }

        private void TrVSheets_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
           VM.SelectedSheet=((Node<ViewSheet>)TrVSheets.SelectedItem).Item;
        }
    }
}
