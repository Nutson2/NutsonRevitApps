using Autodesk.Revit.DB;
using System;
using System.Linq;
using NRPUtils.MVVMBase;


namespace FamilyParameterEditor.EditFamiliesParameters.ViewModel
{
    public class FamilyModel : NotifyObject
    {
        #region private
        private readonly Family family;
        private string          value;
        private string          newFormula;
        private string          name;
        private string          typeName;
        private string          existFormula;
        private FamilyParameter param;
        #endregion
        public Document famDoc;

        #region property
        public string Name         { get => name; set { name = value; OnPropertyChanged(); } }
        public string TypeName     { get => typeName; set { typeName = value; OnPropertyChanged(); } }
        public string Value        { get => value; set { this.value = value; OnPropertyChanged(); } }
        public string NewFormula   { get => newFormula; set { newFormula = value; OnPropertyChanged(); } }
        public string ExistFormula { get => existFormula; set { existFormula = value; OnPropertyChanged(); } }

        #endregion

        public FamilyModel(Family Family)
        {
            family       = Family;
            Name         = family.Name;
        }
        public Document OpenFamily(Document document)
        {
            try
            {
                famDoc = document.EditFamily(family);
                return famDoc;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public void SetDefinition(Document document, Definition definition)
        {
            if (famDoc is null || definition is null) return;
            var elParams = famDoc.FamilyManager.GetParameters();
            param = elParams.Where(x => x.Definition.Name == definition.Name).FirstOrDefault();
            if (param == null) return;

            Value = famDoc.FamilyManager.CurrentType.AsString(param) ?? string.Empty;
            ExistFormula = param.Formula ?? string.Empty;
        }
        public void ApplyNewFormula(Document document)
        {
            if (string.IsNullOrEmpty(newFormula) || param is null) return;
            famDoc?.FamilyManager.SetFormula(param, "\"" + newFormula + "\"");
            famDoc.LoadFamily(document, new FamilyLoadOption());

        }
    }
}
