using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FamilyParameterEditor
{
    public class EditorFamiliesParameters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var RevitTask=new RevitTask();

            var view=new EditFamiliesParameters.View.EditFamilyFormulaView(commandData, RevitTask);
            view.Show();

            return Result.Succeeded;
        }

        public static IEnumerable<Family> GetFamiliesInDocument(Document doc, BuiltInCategory builtInCategory)
        {
            var res=new List<Family>();
            var filter=new ElementCategoryFilter(builtInCategory);

            var coll = new FilteredElementCollector(doc);
            res = coll.OfCategory(builtInCategory)
                    .WhereElementIsElementType()
                    .Cast<FamilySymbol>()
                    .GroupBy(x => x.Family.Name)
                    .Select(x => x.First().Family)
                    .Where(x => x.IsEditable)
                    .ToList();

            return res;
        }

        private void Application_DialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            switch (args)
            {
                // (Konrad) Dismiss Unresolved References pop-up.
                case TaskDialogShowingEventArgs args2:
                    if (args2.DialogId == "TaskDialog_Unresolved_References")
                        args2.OverrideResult(1002);
                    break;
                case DialogBoxShowingEventArgs args3:
                    if (args3.DialogId == "Dialog_Revit_DocWarnDialog")
                        args3.OverrideResult((int)DialogResult.OK);
                    break;
                default:
                    return;
            }
        }

        public void DoFailureProcessing(object sender, FailuresProcessingEventArgs args)
        {
            FailuresAccessor fa = args.GetFailuresAccessor();         // Inside event handler, get all warnings
            IList<FailureMessageAccessor> a = fa.GetFailureMessages();
            int count = 0;
            foreach (FailureMessageAccessor failure in a)
            {
                TaskDialog.Show("Failure", failure.GetDescriptionText()); fa.ResolveFailure(failure);
                ++count;
            }
            if (0 < count && args.GetProcessingResult() == FailureProcessingResult.Continue)
            {
                args.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
            }
        }
    }
}
