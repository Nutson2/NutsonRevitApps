using Autodesk.Revit.DB;
using NRPUtils.MVVMBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace LookupTableEditor
{
    class LookupTableViewModel : NotifyObject
    {
        private Document _doc { get; set; }
        private FamilySizeTableManager FamilySizeTableManager { get; set; }
        public SizeTableBase SizeTableUtility { get; set; }
        private SelectParamViewModel _selectParamViewModel;
        private bool IsThisCreatedTable;

        private List<string> _sizeTableNames;
        public List<string> SizeTableNames
        {
            get
            {
                if (FamilySizeTableManager != null)
                {
                    _sizeTableNames = FamilySizeTableManager.GetAllSizeTableNames().ToList();
                }
                return _sizeTableNames;
            }
        }

        private string curTableName;
        public string CurTableName
        {
            get { return curTableName; }
            set
            {
                curTableName = value;
                if (IsThisCreatedTable)
                {
                    SizeTableUtility = new SizeTableNew(_doc, _selectParamViewModel);
                }
                else
                {
                    SizeTableUtility = new SizeTableExist(_doc, value, FamilySizeTableManager);
                }
                DataTable = SizeTableUtility.AsDataTable;
                OnPropertyChanged();
            }
        }

        private DataTable dataTable;
        public DataTable DataTable
        {
            get { return dataTable; }
            set
            {
                dataTable = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public LookupTableViewModel(Document doc)
        {
            _doc = doc;
            IsThisCreatedTable = false;
            FamilySizeTableManager = FamilySizeTableManager.GetFamilySizeTableManager(_doc, _doc.OwnerFamily.Id);

        }
        public void SaveSizeTable()
        {
            IExportImportSizeTable exportImportSizeTable = SizeTableUtility as IExportImportSizeTable;
            exportImportSizeTable.ExportSizeTableAsCSV();
            Process.Start(SizeTableUtility.PathOnTheDisk);
        }
        public void SetNewTable()
        {
            CheckFamilyType(_doc);

            IExportImportSizeTable exportImportSizeTable = SizeTableUtility as IExportImportSizeTable;
            exportImportSizeTable.ExportSizeTableAsCSV();
            exportImportSizeTable.ImportSizeTable(FamilySizeTableManager);
            IsThisCreatedTable = false;

        }
        public void CreateNewTable(SelectParamViewModel SelectParamViewModel)
        {
            _selectParamViewModel = SelectParamViewModel;
            IsThisCreatedTable = true;
            CurTableName = _selectParamViewModel.NameTable;
        }
        public void AddRowOnTop()
        {
            if (DataTable == null) { return; }

            DataTable.Rows.InsertAt(DataTable.NewRow(), 0);
        }
        public void PasteFromClipboard(int rowIndx, int columnIndx)
        {
            if (DataTable == null) { return; }
            if (!Clipboard.ContainsText()) { return; }

            int tmpColIndx=0;
            int tmpRowIndx=0;
            DataTable dataTable = DataTable;

            var clipboardContent = Clipboard.GetText();
            var rows = clipboardContent.Split(new string[] { "\r\n" }, StringSplitOptions.None)
                                       .Where(x => !string.IsNullOrEmpty(x))
                                       .ToList();

            foreach (string row in rows)
            {
                if (rowIndx + tmpRowIndx >= dataTable.Rows.Count)
                {
                    dataTable.Rows.Add(dataTable.NewRow());
                }
                tmpColIndx = 0;
                string[] columnsValue = row.Split('\t');
                foreach (string columnValue in columnsValue)
                {

                    if (columnIndx + tmpColIndx >= dataTable.Columns.Count) { break; }

                    try
                    {

                        if (dataTable.Columns[tmpColIndx].DataType == Type.GetType("System.String"))
                        {
                            dataTable.Rows[rowIndx + tmpRowIndx][columnIndx + tmpColIndx] = columnValue.ToString();
                        }
                        else
                        {
                            dataTable.Rows[rowIndx + tmpRowIndx][columnIndx + tmpColIndx] = double.Parse(columnValue);
                        }

                    }
                    catch { }
                    tmpColIndx++;
                }
                tmpRowIndx++;

            }

            DataTable = dataTable;
        }
        public static void CheckFamilyType(Document _doc)
        {
            FamilyManager familyManager = _doc.FamilyManager;
            FamilyType    currentType   = familyManager.CurrentType;
            FamilyTypeSet types         = familyManager.Types;

            if (types.IsEmpty || currentType.Name == " ")
            {
                using (Transaction tr = new Transaction(_doc, "Создание типоразмера"))
                {
                    tr.Start();
                    FamilyType currentType2 = familyManager.NewType(_doc.Title);
                    familyManager.CurrentType = currentType2;
                    _doc.Regenerate();
                    tr.Commit();
                }
            }

        }


    }
}
