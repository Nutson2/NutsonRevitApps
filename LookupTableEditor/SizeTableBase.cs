using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace LookupTableEditor
{
    public abstract class SizeTableBase
    {
        public string PathOnTheDisk;
        public DataTable AsDataTable;

        protected readonly string headerDelimiter = ",";
        protected readonly Document Doc;
        protected  FamilySizeTable FamilySizeTable;
        protected string systemDecimalSeparator;

        public string TableName { get; set; }
        public List<FamilyParameter> KeyParameters { get; set; }
        public List<SizeTableDependedParameterModel> DependedParameters { get; set; }
        public FamilyParameter ParamStorageTableName { get; set; }
        public string ParamNameStorageTableName { get; set; }
        public SizeTableBase(Document document)
        {
            Doc = document;
            KeyParameters = new List<FamilyParameter>();
            DependedParameters = new List<SizeTableDependedParameterModel>();
            systemDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        }

        protected void SaveSizeTableOnTheDisk(List<string> stringsToWrite)
        {
            string folderPath = Doc.PathName == "" ?
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) :
                    Path.GetDirectoryName(Doc.PathName);

            PathOnTheDisk = Path.Combine(folderPath, TableName + ".csv");

            using (StreamWriter sw = new StreamWriter(PathOnTheDisk, false, Encoding.Default))
            {
                foreach (string stringRow in stringsToWrite)
                {
                    sw.WriteLine(stringRow);
                }
            }
        }

        protected List<string> GetBodySizeTable()
        {
            List<string> bodyRows= new List<string>();

            for (int rowNum = 0; rowNum < AsDataTable.Rows.Count; rowNum++)
            {
                string valRow = "";
                foreach (DataColumn column in AsDataTable.Columns)
                {
                    if (column.DataType == Type.GetType("System.String"))
                    {
                        valRow += string.Format("\"{0} \"" + headerDelimiter,
                            AsDataTable.Rows[rowNum][column].ToString().Replace("\"", "\"\""));
                    }
                    else
                    {
                        valRow += string.Format("{0}" + headerDelimiter,
                            AsDataTable.Rows[rowNum][column].ToString().Replace(systemDecimalSeparator, "."));
                    }

                }
                valRow = valRow.Remove(valRow.Length - 1);
                bodyRows.Add(valRow);
            }
            return bodyRows;
        }

        protected void SetSizeTable(FamilySizeTableManager FamilySizeTableManager)
        {
            using (Transaction tr = new Transaction(Doc, "Импорт новой таблицы"))

            {
                tr.Start();
                Doc.Regenerate();
                try
                {
                    FamilySizeTableManager.CreateFamilySizeTableManager(Doc, Doc.OwnerFamily.Id);
                    FamilySizeTableManager = FamilySizeTableManager.GetFamilySizeTableManager(Doc, Doc.OwnerFamily.Id);

                    FamilySizeTableErrorInfo errorInfo = new FamilySizeTableErrorInfo();

                    FamilySizeTableManager.ImportSizeTable(Doc, PathOnTheDisk, errorInfo);

                    if (errorInfo.FamilySizeTableErrorType != FamilySizeTableErrorType.Undefined)
                    {
                        TaskDialog.Show("Проблема импорта таблицы",
                            errorInfo.FamilySizeTableErrorType.ToString() + "\n"
                            + errorInfo.InvalidHeaderText + "\n"
                            + errorInfo.InvalidColumnIndex + "\n"
                            + errorInfo.InvalidRowIndex);
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Проблема импорта таблицы", e.Message.ToString() + "\n"
                        + e.Source);
                }
                tr.Commit();
            }
            if (File.Exists(PathOnTheDisk)) File.Delete(PathOnTheDisk);
        }
    }
}