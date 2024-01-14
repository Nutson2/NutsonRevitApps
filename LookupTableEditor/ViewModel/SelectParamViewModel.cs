using Autodesk.Revit.DB;
using NRPUtils.MVVMBase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Data;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LookupTableEditor
{
    public class SelectParamViewModel : NotifyObject
    {
        private readonly Document _doc;

        private string _nameTable;

        public string NameTable
        {
            get { return _nameTable; }
            set 
            { 
                _nameTable = value;
                if (_nameTable != string.Empty) { SelectedNameTable = true; }
                OnPropertyChanged();
            }
        }

        private bool selectedNameTable;
        public bool SelectedNameTable
        {
            get { return selectedNameTable; }
            set
            {
                selectedNameTable = value;
                OnPropertyChanged();
            }
        }

        private bool selectedNameTableParam;
        public bool SelectedNameTableParam
        {
            get { return selectedNameTableParam; }
            set 
            { 
                selectedNameTableParam = value;
                OnPropertyChanged();
            }
        }

        private bool selectedKeyParam;
        public bool SelectedKeyParam
        {
            get { return selectedKeyParam; }
            set 
            {
                selectedKeyParam = value;
                OnPropertyChanged();
            }
        }

        private bool selectedDependParam;
        public bool SelectedDependParam
        {
            get { return selectedDependParam; }
            set 
            {
                selectedDependParam = value;
                OnPropertyChanged();
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
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
