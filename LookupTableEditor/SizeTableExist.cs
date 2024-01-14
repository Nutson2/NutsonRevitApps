using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Data;
using NRPUtils;


namespace LookupTableEditor
{
    public class SizeTableExist : SizeTableBase, IExportImportSizeTable
    {
        private readonly Dictionary<string, string> dictSizeTableHeadStrings;

        public SizeTableExist(Document document, string tableName, FamilySizeTableManager familySizeTableManager)
                            : base(document)
        {
            TableName = tableName;
            FamilySizeTable = familySizeTableManager.GetSizeTable(TableName);

            AsDataTable = GetTableData();

            if (Doc.Application.VersionNumber == "2021")
            {
                dictSizeTableHeadStrings = ParametersUnitType.GetDictionaryToConvertSizeTableColumnTypeInHeaderNameString2021();
            }
            else
            {
                dictSizeTableHeadStrings = ParametersUnitType.GetDictionaryToConvertSizeTableColumnTypeInHeaderNameString2020();
            }

        }

        public void ExportSizeTableAsCSV()
        {
            List<string> stringsToWrite = new List<string>();

            stringsToWrite.Add(GetHeaderSizeTable());
            stringsToWrite.AddRange(GetBodySizeTable());
            SaveSizeTableOnTheDisk(stringsToWrite);
        }

        public void ImportSizeTable(FamilySizeTableManager FamilySizeTableManager)
        {
            using (Transaction tr = new Transaction(Doc, "Удаление существующей таблицы"))
            {
                tr.Start();
                    FamilySizeTableManager.RemoveSizeTable(TableName);
                tr.Commit();
            }

            SetSizeTable(FamilySizeTableManager);
        }
        private string GetHeaderSizeTable()
        {
            string headerRow = string.Empty;

            for (int i = 1; i < FamilySizeTable.NumberOfColumns; i++)
            {
                FamilySizeTableColumn column = FamilySizeTable.GetColumnHeader(i);
                if (dictSizeTableHeadStrings.ContainsKey(column.UnitType.ToString()))
                {
                    headerRow += headerDelimiter
                                + column.Name
                                + dictSizeTableHeadStrings[column.UnitType.ToString()];
                }
            }
            return headerRow;
        }
        private DataTable GetTableData()
        {
            DataTable dataTable = new DataTable();

            //Фомирование заголовков
            for (int columnIndex = 0; columnIndex < FamilySizeTable.NumberOfColumns; columnIndex++)
            {
                FamilySizeTableColumn columnHead = FamilySizeTable.GetColumnHeader(columnIndex);
                string ColumnName = columnHead.Name.Replace(".", "_");
                Type ColumnType = columnHead.DisplayUnitType == DisplayUnitType.DUT_UNDEFINED ?
                                                                Type.GetType("System.String") :
                                                                Type.GetType("System.Double");
                dataTable.Columns.Add(ColumnName, ColumnType);
            }
            //Наполнение тела таблицы
            for (int row = 0; row < FamilySizeTable.NumberOfRows; row++)
            {
                var dataRow = dataTable.NewRow();
                dataTable.Rows.Add(dataRow);
                for (int column = 0; column < FamilySizeTable.NumberOfColumns; column++)
                {
                    string val = FamilySizeTable.AsValueString(row, column);

                    if (dataTable.Columns[column].DataType == Type.GetType("System.Double"))
                    {
                        double.TryParse(val.Replace(".", systemDecimalSeparator), out double doubleValue);
                        dataRow[dataTable.Columns[column].ColumnName] = doubleValue;
                    }
                    else
                    {
                        dataRow[dataTable.Columns[column].ColumnName] = val;
                    }
                }
            }

            return dataTable;
        }
    }

}
