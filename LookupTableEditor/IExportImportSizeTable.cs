using Autodesk.Revit.DB;

namespace LookupTableEditor
{
    interface IExportImportSizeTable
    {
        void ExportSizeTableAsCSV();
        void ImportSizeTable(FamilySizeTableManager FamilySizeTableManager);
    }
}