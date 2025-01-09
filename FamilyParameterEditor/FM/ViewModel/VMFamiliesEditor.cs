using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FamilyParameterEditor.FM.Model;

namespace FamilyParameterEditor.FM.ViewModel
{
    public partial class VMFamiliesEditor : ObservableObject
    {
        #region private
        private readonly ExternalCommandData externalCommandData;
        private readonly RevitTask revitTask;
        private readonly Document document;
        private ObservableCollection<FamilyModel> families;

        [ObservableProperty]
        private string _selectedPath;
        #endregion

        #region property

        public ObservableCollection<FamilyModel> Families
        {
            get { return families; }
            set { families = value; }
        }
        public Category SelectedCategory { get; set; }

        #endregion

        #region constructor
        public VMFamiliesEditor(ExternalCommandData ExternalCommandData, RevitTask RevitTask)
        {
            externalCommandData = ExternalCommandData;
            revitTask = RevitTask;
            document = externalCommandData.Application.ActiveUIDocument.Document;

            Families = new ObservableCollection<FamilyModel>();
            FillFamiliesList();
        }

        private void FillFamiliesList(string Path = "")
        {
            Families.Clear();
            List<FamilyModel> tempList = new List<FamilyModel>();

            if (!string.IsNullOrEmpty(Path))
            {
                tempList = GetFamiliesFromFolder(Path);
            }
            else
            {
                if (document.IsFamilyDocument)
                    tempList.Add(new FamilyModel(document, document.OwnerFamily));
                else
                {
                    BuiltInCategory SelectedCategoryEnum = (BuiltInCategory)
                        SelectedCategory.Id.IntegerValue;
                    tempList = GetFamiliesInDocument(document, SelectedCategoryEnum);
                }
            }

            tempList?.ForEach(x => Families.Add(x));
        }

        #endregion

        #region public methods

        #endregion

        #region private methods
        private List<FamilyModel> GetFamiliesInDocument(
            Document doc,
            BuiltInCategory builtInCategory
        )
        {
            var res = new List<FamilyModel>();
            var filter = new ElementCategoryFilter(builtInCategory);

            var coll = new FilteredElementCollector(doc);
            res = coll.OfCategory(builtInCategory)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .GroupBy(x => x.Family.Name)
                .Select(x => x.First().Family)
                .Where(x => x.IsEditable)
                .Select(x => new FamilyModel(x))
                .ToList();

            return res;
        }

        private List<FamilyModel> GetFamiliesFromFolder(string path)
        {
            var res = new List<FamilyModel>();
            List<string> familiesOnSelectedPath = Directory
                .GetFiles(path, "*.rfa", SearchOption.AllDirectories)
                .ToList();
            res = familiesOnSelectedPath.Select(x => new FamilyModel(document, x)).ToList();

            return res;
        }

        [RelayCommand]
        public void SelectFamFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                SelectedPath = dialog.SelectedPath;
            FillFamiliesList(SelectedPath);
        }
        #endregion
    }
}
