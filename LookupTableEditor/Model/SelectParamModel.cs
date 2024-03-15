using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace LookupTableEditor
{
    public partial class SelectParamModel : ObservableObject 
    {
        [ObservableProperty]
        private string _selectedRole;
        public string Name { get; set; }
        public List<string> Role { get; set; }
        public FamilyParameter FamilyParam { get; set; }

        public SelectParamModel(FamilyParameter param)
        {
            FamilyParam = param;
            Name = param.Definition.Name;
            Role = new List<string>() { "Имя таблицы", "Ключевой", "Зависимый", " " };
        }

    }
}
