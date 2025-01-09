using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MEPGadgets.MEPSystemFilters.Model;
using NRPUtils.Extentions;

namespace MEPGadgets.MEPSystemFilters
{
    public partial class VMMEPSystemFilters : ObservableObject
    {
        private readonly Document doc;

        [ObservableProperty]
        private List<PipingSystemType> systemTypes;

        [ObservableProperty]
        private PipingSystem _selectedSystem;

        [ObservableProperty]
        private ParameterElement _filteredParameter;

        [ObservableProperty]
        private PipingSystemType _selectedType;

        [ObservableProperty]
        private string _selectedCategoriesName;

        partial void OnSelectedTypeChanged(PipingSystemType value)
        {
            UpdateSystemsList();
        }

        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public ObservableCollection<ParameterElement> AllowedParametersFromSelectedCategories { get; set; } =
            new ObservableCollection<ParameterElement>();
        public ObservableCollection<PipingSystem> Systems { get; set; } =
            new ObservableCollection<PipingSystem>();
        public ObservableCollection<PipingSystem> SelectedSystems { get; set; } =
            new ObservableCollection<PipingSystem>();
        public ObservableCollection<ModelTaskForFilter> TasksForFilters { get; set; } =
            new ObservableCollection<ModelTaskForFilter>();

        public VMMEPSystemFilters(Document document)
        {
            doc = document;
            SystemTypes = new FilteredElementCollector(doc)
                .WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_PipingSystem))
                .WhereElementIsNotElementType()
                .Select(x => x.GetTypeId())
                .Distinct()
                .Select(x => doc.GetElement(x))
                .Cast<PipingSystemType>()
                .OrderBy(x => x.Name)
                .ToList();
            PrepaireCategoriesList();
        }

        [RelayCommand]
        public void AddSelectedSystem() => SelectedSystems.Add(SelectedSystem);

        [RelayCommand]
        public void CreateTaskForFilter()
        {
            if (SelectedSystems.Count == 0)
                return;
            TasksForFilters.Add(new ModelTaskForFilter(SelectedSystems.ToList()));
            SelectedSystems.Clear();
        }

        [RelayCommand]
        internal void CreateView()
        {
            var element = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).FirstOrDefault();
            var FilteredParameterId = element
                .GetParameters(FilteredParameter.Name)
                .FirstOrDefault()
                ?.Id;
            if (FilteredParameterId == null)
                return;
            var catIds = Categories.Where(x => x.Selected).Select(x => x.Category.Id).ToList();

            doc.DoInTransaction(
                "CreateView",
                () =>
                {
                    foreach (var task in TasksForFilters)
                    {
                        CreateView(doc, catIds, FilteredParameterId, task.Systems);
                        UpdateFilteredParameterValue(FilteredParameter, task.Systems);
                    }
                }
            );
        }

        private void UpdateFilteredParameterValue(
            ParameterElement filteredParameter,
            List<PipingSystem> systems
        )
        {
            foreach (var system in systems)
            {
                foreach (Element el in system.PipingNetwork)
                {
                    var ownerFam = el;
                    if (el is FamilyInstance fInst && fInst.SuperComponent != null)
                        ownerFam = fInst.SuperComponent;

                    el.CopyValueBetweenElements(
                        ownerFam,
                        BuiltInParameter.RBS_SYSTEM_NAME_PARAM,
                        filteredParameter.Name
                    );
                }
            }
        }

        private void UpdateSystemsList()
        {
            Systems.Clear();
            new FilteredElementCollector(doc)
                .WherePasses(new ElementCategoryFilter(BuiltInCategory.OST_PipingSystem))
                .WhereElementIsNotElementType()
                .Where(x => x.GetTypeId() == SelectedType.Id)
                .Cast<PipingSystem>()
                .OrderByDescending(x => x.PipingNetwork.Size)
                .ToList()
                .ForEach(x => Systems.Add(x));
        }

        private void PrepaireCategoriesList()
        {
            foreach (Category curCategory in doc.Settings.Categories)
            {
                if (
                    curCategory.CategoryType != CategoryType.Model
                    || curCategory.Name.Contains(".dwg")
                )
                    continue;

                var categoryModel = new CategoryModel(curCategory);
                Categories.Add(categoryModel);
                categoryModel.PropertyChanged += CategoryModel_PropertyChanged;
            }
            Categories = Categories.OrderBy(x => x.Category.Name).ToList();
        }

        private void CategoryModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Categories.Any())
                return;
            UpdateAllowedParametersList(doc, Categories, AllowedParametersFromSelectedCategories);
            SelectedCategoriesName = string.Join(
                ",\n",
                Categories.Where(x => x.Selected).Select(x => x.Category.Name)
            );
        }

        public void CreateView(
            Document doc,
            List<ElementId> CategoriesId,
            ElementId sharedParameterId,
            List<PipingSystem> pipingSystems
        )
        {
            var filterName = string.Join(",", pipingSystems.Select(x => x.Name));

            var isFilterExist = new FilteredElementCollector(doc)
                .OfClass(typeof(ParameterFilterElement))
                .Any(x => x.Name == filterName);
            if (isFilterExist)
                return;
            var filterList = new List<ElementFilter>();

            foreach (var system in pipingSystems)
            {
                filterList.Add(
                    new ElementParameterFilter(
                        ParameterFilterRuleFactory.CreateNotContainsRule(
                            sharedParameterId,
                            system.Name,
                            false
                        )
                    )
                );
            }
            var andFilter = new LogicalAndFilter(filterList);
            var paramFilter = ParameterFilterElement.Create(
                doc,
                "НЕ " + filterName,
                CategoriesId,
                andFilter
            );

            var newView = View3D.CreateIsometric(
                doc,
                new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .First(x => x.ViewFamily == ViewFamily.ThreeDimensional)
                    .Id
            );
            newView.Name = "Система " + filterName.Replace("НЕ ", "");
            newView.AddFilter(paramFilter.Id);
            newView.SetFilterVisibility(paramFilter.Id, false);
        }

        public void UpdateAllowedParametersList(
            Document Doc,
            List<CategoryModel> categories,
            ObservableCollection<ParameterElement> parameterElements
        )
        {
            List<ElementId> intersectlist = new List<ElementId>();
            categories
                .Where(x => x.Selected)
                .ToList()
                .ForEach(x =>
                {
                    if (intersectlist.Count == 0)
                        intersectlist.AddRange(
                            TableView.GetAvailableParameters(Doc, x.Category.Id)
                        );
                    else
                        intersectlist = intersectlist
                            .Intersect(TableView.GetAvailableParameters(Doc, x.Category.Id))
                            .ToList();
                });

            if (intersectlist == null)
                return;
            intersectlist = intersectlist.Distinct().ToList();

            parameterElements.Clear();
            var paramsList = intersectlist
                .Select(x => Doc.GetElement(x))
                .Cast<ParameterElement>()
                .Where(x => x != null)
                .Where(x => x.GetDefinition().ParameterType == ParameterType.Text)
                .OrderBy(x => x.Name)
                .ToList();

            paramsList.ForEach(x => parameterElements.Add(x));
        }
    }
}
