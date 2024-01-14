using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;



namespace CopyParametersGadgets
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    class CopyParametersToSelectedElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            var doc               = revit.Application.ActiveUIDocument.Document;
            var SelectedIds       =revit.Application.ActiveUIDocument.Selection.GetElementIds();
            if (SelectedIds.Count == 0) return Result.Cancelled;

            var SelectedElBuitlInCategory = (BuiltInCategory)doc.GetElement(SelectedIds.First()).Category.Id.IntegerValue;
            var dataCopyShared            = new DataCopyParameterVM(doc, SelectedElBuitlInCategory);

            if (dataCopyShared.SharedParametersFromGroup(revit.Application.ActiveUIDocument.Selection.GetElementIds()))
            {
                SelectParameters dialog = new SelectParameters(dataCopyShared);
                dialog.ShowDialog();
                return Result.Succeeded;

            }
            else
            {
                TaskDialog taskDialog = new TaskDialog("Ошибка")
                {
                    MainContent = "В модели не найден ни один экземпляр групп",
                    CommonButtons = TaskDialogCommonButtons.Ok
                };
                taskDialog.Show();

                return Result.Failed;
            }

        }

    }
}
