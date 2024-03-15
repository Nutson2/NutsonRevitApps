using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MEPGadgets.MEPSystemFilters.View;
using System.Linq;

namespace MEPGadgets.MEPSystemFilters
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class CreateMEPSystemFilters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            var VM = new VMMEPSystemFilters(doc);
            LoadUserSettings(VM);

            var view = new ViewSelectSystem(VM);
            view.ShowDialog();

            SaveSettings(VM);

            return Result.Succeeded;
        }

        private void LoadUserSettings(VMMEPSystemFilters vM)
        {
            var settings = Settings.Default;
            if (settings.ViewCreaterSelectedCategories != null ||
                settings.ViewCreaterSelectedCategories?.Count > 0)
            {
                vM.Categories.ForEach(c =>
                {
                    if (settings.ViewCreaterSelectedCategories.Contains(c.Category.Name))
                    {
                        c.Selected = true;
                    }
                });
            }
            if (!string.IsNullOrEmpty(settings.ViewCreateSelParameter))
            {
                vM.FilteredParameter = vM.AllowedParametersFromSelectedCategories
                        .Where(x => x.Name == settings.ViewCreateSelParameter)
                        .FirstOrDefault();
            }
        }
        private void SaveSettings(VMMEPSystemFilters vM)
        {
            var settings = Settings.Default;
            settings.ViewCreaterSelectedCategories = new System.Collections.Specialized.StringCollection();
            settings.ViewCreaterSelectedCategories.AddRange(
                vM.Categories.Where(c => c.Selected)
                            .Select(c => c.Category.Name)
                            .ToArray());
            settings.ViewCreateSelParameter = vM.FilteredParameter?.Name;
            settings.Save();
        }
    }
}
