using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NRPUtils;

namespace LookupTableEditor
{
    public class SizeTableNew : SizeTableBase, IExportImportSizeTable
    {
        private readonly Dictionary<string, string> dictSizeTableHeadStrings;

        public SizeTableNew(Document document, SelectParamViewModel selectParamViewModel)
                        : base(document)
        {

            TableName = selectParamViewModel.NameTable;

            var el = (from elem in selectParamViewModel.ListParam
                      where elem.SelectedRole == "Имя таблицы"
                      select elem).First();
            ParamStorageTableName = el.FamilyParam;
            ParamNameStorageTableName = el.FamilyParam.Definition.Name;


            foreach (SelectParamModel element in selectParamViewModel.ListParam)
            {
                switch (element.SelectedRole)
                {
                    case "Имя таблицы":
                        ParamStorageTableName = element.FamilyParam;
                        ParamNameStorageTableName = element.FamilyParam.Definition.Name;
                        break;
                    case "Ключевой":
                        KeyParameters.Add(element.FamilyParam);
                        break;
                    case "Зависимый":
                        DependedParameters.Add(new SizeTableDependedParameterModel(element.FamilyParam, this));
                        break;

                    default:
                        break;
                }

            }
            AsDataTable = GetTableData();


            if (Doc.Application.VersionNumber == "2021")
            {
                dictSizeTableHeadStrings = ParametersUnitType.GetDictionaryToConvertParamTypeInSizeTableHeaderString2021();
            }
            else
            {
                dictSizeTableHeadStrings = ParametersUnitType.GetDictionaryToConvertParamTypeInSizeTableHeaderString2020();
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
            SetSizeTable(FamilySizeTableManager);
            var failedFormulas = new StringBuilder();

            using (Transaction tr = new Transaction(Doc, "Запись формул"))
            {
                tr.Start();

                Doc.FamilyManager.Set(ParamStorageTableName, TableName);

                foreach (SizeTableDependedParameterModel param in DependedParameters)
                {
                    string formula = param.CreateFormula();
                    try
                    {
                        Doc.FamilyManager.SetFormula(param.Parameter, formula);
                    }
                    catch (Exception)
                    {
                        failedFormulas.AppendLine($"Не удалось присвоить формулу: {formula}, параметру {param.Parameter.Definition.Name}");
                    }
                }
                tr.Commit();
            }

            TaskDialog.Show("Результат\n",failedFormulas.ToString());
        }
        private string GetHeaderSizeTable()
        {
            string headerRow = string.Empty;
            List<FamilyParameter> keyToRemove = new List<FamilyParameter>();
            List<SizeTableDependedParameterModel> dependToRemove = new List<SizeTableDependedParameterModel>();

            foreach (FamilyParameter keyParam in KeyParameters)
            {
                var paramTypeName = keyParam.Definition.ParameterType.ToString();
                if (dictSizeTableHeadStrings.ContainsKey(paramTypeName))
                {
                    headerRow += headerDelimiter +
                                keyParam.Definition.Name +
                                dictSizeTableHeadStrings[paramTypeName];
                }
                else
                {
                    keyToRemove.Add(keyParam);
                }
            }
            foreach (SizeTableDependedParameterModel dependParam in DependedParameters)
            {
                var paramTypeName = dependParam.Parameter.Definition.ParameterType.ToString();
                if (dictSizeTableHeadStrings.ContainsKey(paramTypeName))
                {
                    headerRow += headerDelimiter +
                                dependParam.Parameter.Definition.Name +
                                dictSizeTableHeadStrings[paramTypeName];
                }
                else
                {
                    dependToRemove.Add(dependParam);
                }
            }
            foreach (var item in keyToRemove)
            {
                AsDataTable.Columns.Remove(AsDataTable.Columns[item.Definition.Name.Replace(".", "_")]);

                KeyParameters.Remove(item);
            }
            foreach (var item in dependToRemove)
            {
                AsDataTable.Columns.Remove(AsDataTable.Columns[item.Parameter.Definition.Name.Replace(".", "_")]);
                DependedParameters.Remove(item);
            }

            return headerRow;
        }

        private DataTable GetTableData()
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add(string.Empty, Type.GetType("System.String"));
            var zip = new List<FamilyParameter>[] { KeyParameters, 
                (from p in DependedParameters select p.Parameter).ToList()};
            for (int i = 0; i < zip.Length; i++)
            {
                foreach (FamilyParameter param in zip[i])
                {
                    string ColumnName = param.Definition.Name.Replace(".","_");
                    Type ColumnType = param.Definition.ParameterType == ParameterType.Text ?
                                                                    Type.GetType("System.String") :
                                                                    Type.GetType("System.Double");
                    dataTable.Columns.Add(ColumnName, ColumnType);
                }

            }
            var dataRow = dataTable.NewRow();
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.DataType == Type.GetType("System.Double"))
                {
                    dataRow[column.ColumnName] = 0;
                }
                else
                {
                    dataRow[column.ColumnName] = string.Empty;
                }
            }
            dataTable.Rows.Add(dataRow);
            return dataTable;
        }
    }

}
