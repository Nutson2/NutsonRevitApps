using Autodesk.Revit.DB;
using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FamilyParameterEditor.EditFamiliesParameters.ViewModel
{
    public partial class FamilyModel : ObservableObject
    {
        #region private
        [ObservableProperty]
        private string _value;
        [ObservableProperty]
        private string _newFormula;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _typeName;
        [ObservableProperty]
        private string _existFormula;

        private readonly Family family;
        private FamilyParameter param;
        #endregion

        public Document famDoc;
        public FamilyModel(Family Family)
        {
            family = Family;
            Name   = family.Name;
        }
        public Document OpenFamily(Document document)
        {
            try
            {
                famDoc = document.EditFamily(family);
                return famDoc;
            }
            catch (Exception) { return null; }
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
            if (string.IsNullOrEmpty(NewFormula) || param is null) return;
            famDoc?.FamilyManager.SetFormula(param, "\"" + NewFormula + "\"");
            famDoc.LoadFamily(document, new FamilyLoadOption());

        }
    }
}
