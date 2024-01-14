using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NRPUtils.MVVMBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyParameterEditor.EditFamiliesParameters.ViewModel
{
    public class VMEditFamiliesParameters : NotifyObject, IDisposable
    {
        #region private
        private DefinitionFile SHF;
        private Category selectedCategory;
        private Definition definition;
        private DefinitionGroup selectedGroup;
        private readonly List<Document> familiesDocuments= new List<Document>();
        #endregion

        #region collections

        public ObservableCollection<FamilyModel> Families { get; set; } = new ObservableCollection<FamilyModel>();
        public ObservableCollection<DefinitionGroup> SharedParametersGroup { get; set; } = new ObservableCollection<DefinitionGroup>();
        public ObservableCollection<Definition> SharedParametersDefinitions { get; set; } = new ObservableCollection<Definition>();
        public List<Category> Category { get; set; } = new List<Category>();
        #endregion

        #region property    
        public RevitTask RevitTask { get; set; }
        public Document Document { get; set; }
        public Category SelectedCategory { get => selectedCategory; set { selectedCategory = value; OnPropertyChanged(); } }
        public DefinitionGroup SelectedGroup { get => selectedGroup; set { selectedGroup = value; OnPropertyChanged(); } }
        public Definition SelectedDefinition { get => definition; set { definition = value; OnPropertyChanged(); } }
        #endregion

        public VMEditFamiliesParameters(ExternalCommandData commandData, RevitTask revitTask)
        {
            Document = commandData.Application.ActiveUIDocument.Document;
            RevitTask = revitTask;

            var EnumCategory = new List<BuiltInCategory>()
                    {
                        BuiltInCategory.OST_PipeAccessory,
                        BuiltInCategory.OST_PlumbingFixtures,
                        BuiltInCategory.OST_PipeFitting,
                        BuiltInCategory.OST_MechanicalEquipment
                    };
            EnumCategory.ForEach(x => { Category.Add(Autodesk.Revit.DB.Category.GetCategory(Document, x)); });

            selectedCategory = Category.First();

            Init();
            PropertyChanged += SelectedGroup_PropertyChanged;
            PropertyChanged += SelectedCategory_PropertyChanged;
            PropertyChanged += SelectedDefinition_PropertyChanged;
        }
        public void ApplyNewFormula()
        {
            RevitTask.Run(uiApp =>
            {
                foreach (var f in Families)
                {
                    using (var tr = new Transaction(f.famDoc, f.Name))
                    {
                        tr.Start();

                        f.ApplyNewFormula(Document);

                        tr.Commit();
                    }
                }
            });
        }
        public void Dispose()
        {
            RevitTask.Run(uiApp =>
            {
                familiesDocuments.ForEach(f => 
                {
                    if (f != null && f.IsValidObject)
                    { 
                        f.Close(false);
                        f.Dispose(); 
                    }
                });
                familiesDocuments.Clear();
            });
        }

        #region private methods
        private void Init()
        {
            SHF = Document.Application.OpenSharedParameterFile();
            SHF.Groups.ToList().ForEach(g => { SharedParametersGroup.Add(g); });
            FillDefinitionFromGroup();
            FillFamilies();
        }
        private void FillFamilies()
        {
            Families.Clear();

            EditorFamiliesParameters.GetFamiliesInDocument(Document, (BuiltInCategory)selectedCategory.Id.IntegerValue)
                .Select(x => new FamilyModel(x))
                .ToList()
                .ForEach(x =>
                {
                    familiesDocuments.Add(x.OpenFamily(Document));
                    Families.Add(x);
                });
            if (SelectedDefinition is null) return;
            FillFamiliesParameterValue(Document, SelectedDefinition);
        }
        private void FillDefinitionFromGroup()
        {
            SharedParametersDefinitions.Clear();
            SelectedGroup ??= SharedParametersGroup.FirstOrDefault();
            SelectedGroup.Definitions
                .ToList()
                .ForEach(i => SharedParametersDefinitions.Add(i));
        }
        private void FillFamiliesParameterValue(Document document, Definition selectedDefinition)
        {
            foreach (var f in Families)
            {
                f.SetDefinition(document, selectedDefinition);
            }
        }
        #endregion

        #region handlers
        private void SelectedGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SelectedGroup)) return;
            FillDefinitionFromGroup();
        }
        private void SelectedCategory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SelectedCategory)) return;
            FillFamilies();
        }
        private void SelectedDefinition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SelectedDefinition)) return;
            FillFamiliesParameterValue(Document, SelectedDefinition);
        }


        #endregion

    }
}
