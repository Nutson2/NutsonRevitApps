using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using LookupTableEditor;

namespace LookupTableEditor
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    class LookupTableEditorECommand : IExternalCommand

    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            Document doc = revit.Application.ActiveUIDocument.Document;

            if (!doc.IsFamilyDocument || app.Application.VersionNumber == "2022"
                || app.Application.VersionNumber == "2023" || app.Application.VersionNumber == "2024")
                     return Result.Succeeded;

            LookupTableView lookupTableForm = new LookupTableView(doc);
            try
            {
                lookupTableForm.ShowDialog();

            }
            finally
            {
                lookupTableForm.Close();

            }
            return Result.Succeeded;

        }
    }

}
