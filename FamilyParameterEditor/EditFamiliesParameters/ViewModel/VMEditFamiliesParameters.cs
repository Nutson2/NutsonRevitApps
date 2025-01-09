using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FamilyParameterEditor.EditFamiliesParameters.ViewModel
{
    public partial class VMEditFamiliesParameters : ObservableObject, IDisposable
    {
        #region private
        private DefinitionFile SHF;
        private readonly List<Document> familiesDocuments = new List<Document>();

        [ObservableProperty]
        private Category _selectedCategory;

        [ObservableProperty]
        private Definition _definition;

        [ObservableProperty]
        private DefinitionGroup _selectedGroup;
        #endregion

        #region collections

        public ObservableCollection<FamilyModel> Families { get; set; } =
            new ObservableCollection<FamilyModel>();
        public ObservableCollection<DefinitionGroup> SharedParametersGroup { get; set; } =
            new ObservableCollection<DefinitionGroup>();
        public ObservableCollection<Definition> SharedParametersDefinitions { get; set; } =
            new ObservableCollection<Definition>();
        public List<Category> Category { get; set; } = new List<Category>();
        #endregion

        #region property
        public RevitTask RevitTask { get; set; }
        public Document Document { get; set; }
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
                BuiltInCategory.OST_MechanicalEquipment,
            };
            EnumCategory.ForEach(x =>
            {
                Category.Add(Autodesk.Revit.DB.Category.GetCategory(Document, x));
            });

            SelectedCategory = Category.First();

            Init();
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
            SHF.Groups.ToList()
                .ForEach(g =>
                {
                    SharedParametersGroup.Add(g);
                });
            FillDefinitionFromGroup();
            FillFamilies();
        }

        private void FillFamilies()
        {
            Families.Clear();

            EditorFamiliesParameters
                .GetFamiliesInDocument(Document, ToBuiltInCategory(SelectedCategory))
                .Select(x => new FamilyModel(x))
                .ToList()
                .ForEach(x =>
                {
                    familiesDocuments.Add(x.OpenFamily(Document));
                    Families.Add(x);
                });
            if (Definition is null)
                return;
            FillFamiliesParameterValue(Document, Definition);
        }

        private BuiltInCategory ToBuiltInCategory(Category SelectedCategory)
        {
            return (BuiltInCategory)SelectedCategory.Id.IntegerValue;
        }

        private void FillDefinitionFromGroup()
        {
            SharedParametersDefinitions.Clear();
            SelectedGroup ??= SharedParametersGroup.FirstOrDefault();
            SelectedGroup.Definitions.ToList().ForEach(i => SharedParametersDefinitions.Add(i));
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
        partial void OnSelectedGroupChanged(DefinitionGroup value)
        {
            FillDefinitionFromGroup();
        }

        partial void OnSelectedCategoryChanged(Category value)
        {
            FillFamilies();
        }

        partial void OnDefinitionChanged(Definition value)
        {
            FillFamiliesParameterValue(Document, Definition);
        }

        #endregion
    }
}
