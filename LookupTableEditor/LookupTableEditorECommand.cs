using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NRPUtils.Extentions;

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

            if (!doc.IsFamilyDocument || app.Application.VersionNumber.ToInt() > 2022)
                return Result.Cancelled;

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
