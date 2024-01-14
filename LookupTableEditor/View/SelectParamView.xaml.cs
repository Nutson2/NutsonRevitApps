using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;

namespace LookupTableEditor
{
    public partial class SelectParamView : Window
    {
        readonly SelectParamViewModel context;
        readonly LookupTableView tableForm;
        public SelectParamView(Document doc, LookupTableView lookupTableForm)
        {
            InitializeComponent();
            tableForm = lookupTableForm;
            context = new SelectParamViewModel(doc);
            DataContext = context;
            lb_paramsList.ItemsSource = context.ListParam;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            ListBoxItem container =lb_paramsList.ContainerFromElement(comboBox) as ListBoxItem;
            if (container==null) return;

            SelectParamModel el= container.Content as SelectParamModel;
            if (el == null)  return; 

            if (!lb_paramsList.SelectedItems.Contains(el))  lb_paramsList.SelectedItems.Add(el); 

            for (int i = 0; i < lb_paramsList.SelectedItems.Count; i++)
            {
                ((SelectParamModel)lb_paramsList.SelectedItems[i]).SelectedRole= comboBox.SelectedValue.ToString();
            }
            context.CheckCheckBox();
            lb_paramsList.SelectedItems.Clear();
        }

        private void bt_Accept_Click(object sender, RoutedEventArgs e)
        {
            if (context.SelectedNameTableParam && 
                context.SelectedKeyParam && 
                context.SelectedDependParam &&
                context.SelectedNameTable)
            {
                tableForm.SelectParamsForNewTable = context.GetSelectParamsForNewTable();
                this.Close();
            }
        }
    }
}
