using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LookupTableEditor
{
    public partial class SelectParamViewModel : ObservableObject
    {
        private readonly Document _doc;

        [ObservableProperty]
        private string _nameTable;
        [ObservableProperty]
        private bool _selectedNameTable;
        [ObservableProperty]
        private bool _selectedNameTableParam;
        [ObservableProperty]
        private bool _selectedKeyParam;
        [ObservableProperty]
        private bool _selectedDependParam;

        public ObservableCollection<SelectParamModel> ListParam;

        public SelectParamViewModel(Document document)
        {
            _doc = document;
            ListParam = new ObservableCollection<SelectParamModel>();

            List<FamilyParameter> listParameters = (from FamilyParameter item in _doc.FamilyManager.Parameters
                                                    select item).OrderByDescending(item=>item.Definition.Name).ToList();
            for (int i = listParameters.Count - 1; i >= 0; i--)
            {
                SelectParamModel paramModel = new SelectParamModel(listParameters[i]);
                ListParam.Add(paramModel);
            }
        }
        partial void OnNameTableChanged(string value)
        {
            if (value != string.Empty) { SelectedNameTable = true; }
        }

        public void CheckCheckBox()
        {
            SelectedNameTableParam = false;
            SelectedKeyParam = false;
            SelectedDependParam = false;

            foreach (var element in ListParam)
            {
                switch (element.SelectedRole)
                {
                    case "Имя таблицы":
                        SelectedNameTableParam = !SelectedNameTableParam;
                        break;
                    case "Ключевой":
                        SelectedKeyParam = true;
                        break;
                    case "Зависимый":
                        SelectedDependParam = true;
                        break;

                    default:
                        break;
                }
            }
        }
        public SelectParamViewModel GetSelectParamsForNewTable()
        {
            return this;
        }
    }
}
