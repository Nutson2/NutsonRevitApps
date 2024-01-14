using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CopyParametersGadgets
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    class CopyParametersToIsolation : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            var doc            = revit.Application.ActiveUIDocument.Document;
            var dataCopyShared = new DataCopyParameterIsolationVM(doc);

            if (dataCopyShared.PrepareParameters())
            {
                SelectParameters dialog = new SelectParameters(dataCopyShared);
                dialog.ShowDialog();
                return Result.Succeeded;

            }
            else
            {
                TaskDialog taskDialog = new TaskDialog("Ошибка")
                {
                    MainContent = "Не найдены элементы изоляции в модели",
                    CommonButtons = TaskDialogCommonButtons.Ok
                };
                taskDialog.Show();

                return Result.Failed;
            }

        }
    }
}
