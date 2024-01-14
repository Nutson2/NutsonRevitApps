using Autodesk.Revit.DB;
using MEPGadgets.Scheme.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MEPGadgets.Scheme.View
{
    /// <summary>
    /// Логика взаимодействия для SchemeView.xaml
    /// </summary>
    public partial class SchemeView : Window
    {
        readonly SystemScheme schemeVM = null;
        public SchemeView(SystemScheme SchemeVM)
        {
            InitializeComponent();
            schemeVM = SchemeVM;
            treeView.ItemsSource = schemeVM.Branches;
            LBox.ItemsSource = schemeVM.BrancheEnds;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            ICollection<ElementId> selElementsId = GetSelectedItemId(e.NewValue);
            schemeVM.UIDoc.Selection.SetElementIds(selElementsId);
        }

        private void TreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ICollection<ElementId> selElementsId = GetSelectedItemId(treeView.SelectedItem);
            schemeVM.UIDoc.ShowElements(selElementsId);
        }
        private static ICollection<ElementId> GetSelectedItemId(object obj)
        {
            ICollection<ElementId> selElementsId = default;
            if (obj is SchemeBranch selBranch)
            {
                selElementsId = selBranch.Elements.Select(x => x.RevitElement.Id).ToList();
            }
            else if (obj is SchemeElement selEl)
            {
                selElementsId = new List<ElementId>
                {
                    selEl.RevitElement.Id
                };
            }

            return selElementsId;
        }

        private void LBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (SchemeElement item in e.AddedItems)
            {
                item.IsSelected = true;
                item.IsExpanded = true;
                item.Branch.IsExpanded = true;
            }
        }
        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.BringIntoView();
                e.Handled = true;
            }
        }
    }
}
