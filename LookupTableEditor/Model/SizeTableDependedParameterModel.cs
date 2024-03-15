using Autodesk.Revit.DB;
using System.Linq;


namespace LookupTableEditor
{
    public class SizeTableDependedParameterModel
    {
        public FamilyParameter Parameter { get; set; }
        public string ColumnName { get; set; }
        public string TableName { get; set; }
        public string DefaultValue { get; set; }
        public string ExistFormula { get; set; }
        private readonly SizeTableBase _sizeTableUtility;
        public SizeTableDependedParameterModel(FamilyParameter parameter, SizeTableBase sizeTableUtility)
        {
            _sizeTableUtility = sizeTableUtility;
            Parameter = parameter;

        }
        public string CreateFormula()
        {
            if (Parameter.Formula != null) return Parameter.Formula;

            //TableName  = $"{_sizeTableUtility.ParamNameStorageTableName}";
            TableName = $"\"{_sizeTableUtility.TableName}\"";
            ColumnName = Parameter.Definition.Name;
            DefaultValue = _sizeTableUtility.AsDataTable.Rows[0][ColumnName.Replace(".", "_")].ToString().Replace("\"", "");

            string keys=string.Empty;
            _sizeTableUtility.KeyParameters.Select(x => x.Definition.Name).ToList().ForEach(x => keys += $", {x}");

            var res=$"size_lookup({TableName}, \"{ColumnName}\", \"{DefaultValue}\" {keys})";
            return res;
        }
    }
}
