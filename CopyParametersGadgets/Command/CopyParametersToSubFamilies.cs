using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CopyParametersGadgets
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    class CopyParametersToSubFamilies : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            var doc            = revit.Application.ActiveUIDocument.Document;
            var dataCopyShared = new DataCopyParameterSubFamiliesVM(doc);

            if (dataCopyShared.PrepareParentFamilyParameters(revit.Application.ActiveUIDocument.Selection.GetElementIds()))
            {
                SelectParameters dialog = new SelectParameters(dataCopyShared);
                dialog.ShowDialog();
                return Result.Succeeded;

            }
            else
            {
                TaskDialog taskDialog = new TaskDialog("Ошибка")
                {
                    MainContent = "Необходимо выбрать элементы перед запуском",
                    CommonButtons = TaskDialogCommonButtons.Ok
                };
                taskDialog.Show();

                return Result.Failed;
            }

        }
    }
}
