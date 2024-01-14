using Autodesk.Revit.DB;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LookupTableEditor
{
    public partial class LookupTableView : Window
    {
        readonly LookupTableViewModel data;
        readonly Document _doc;

        public SelectParamViewModel SelectParamsForNewTable;
        public LookupTableView(Document doc)
        {
            _doc = doc;
            InitializeComponent();
            data = new LookupTableViewModel(doc);
            DataContext = data;
            
        }


        private void ChangeTable_Click(object sender, RoutedEventArgs e)
        {
            if (data.DataTable == null) { return; }

            data.SetNewTable();
            this.Close();
        }

        private void CreateNewTable_Click(object sender, RoutedEventArgs e)
        {

            SelectParamView selectParamView = new SelectParamView(_doc,this);
            selectParamView.ShowDialog();
            if (SelectParamsForNewTable != null)
            {
                data.CreateNewTable(SelectParamsForNewTable);
            }
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {

            data.AddRowOnTop();
        }

        private void PasteToDataGrid()
        {
            if (data.DataTable == null) { return; }

            var cellInfo = dg_Table.SelectedCells;
            var columnNum = cellInfo.First().Column.DisplayIndex;
            int rowNum = 0;

            var rowView = cellInfo.First().Item as DataRowView;
            if (rowView!=null)
            {
                for (int rowIndex = 0; rowIndex < rowView.DataView.Count; rowIndex++)
                {
                    if (rowView == rowView.DataView[rowIndex])
                    {
                        rowNum = rowIndex;
                        break;
                    }
                }
            }
            else
            {
                data.DataTable.Rows.Add(data.DataTable.NewRow());
                rowNum = data.DataTable.Rows.Count-1;
            }
            data.PasteFromClipboard(rowNum, columnNum);
        }

        private void GotoTelegram_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://t.me/Nutson2");
        }

        private void GoToYouTube_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/channel/UCcBEuUPtAW22nkZKueGD0VQ");
        }

        private void GoToQivi_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://qiwi.com/n/NUTSON");

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.V && Keyboard.Modifiers==ModifierKeys.Control)
            {
                PasteToDataGrid();
            }
        }

        private void ExportTable_Click(object sender, RoutedEventArgs e)
        {
            data.SaveSizeTable();

        }
    }
}
