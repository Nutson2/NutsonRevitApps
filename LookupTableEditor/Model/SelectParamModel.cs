using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using NRPUtils.MVVMBase;

namespace LookupTableEditor
{
    public class SelectParamModel : NotifyObject
    {
        public string Name { get; set; }
        private string selectedRole;
        public string SelectedRole 
        {
            get { return selectedRole; }
            set
            {
                selectedRole = value;
                OnPropertyChanged();
            }
        }
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
