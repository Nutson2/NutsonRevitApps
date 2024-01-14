using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace CopyParametersGadgets.Command
{
    #region Assist classes
    public static class ViewSheetExtensions
    {
        public static IEnumerable<ViewSchedule> GetSchedules(this ViewSheet viewSheet)
        {
            var doc = viewSheet.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc, viewSheet.Id);
            var scheduleSheetInstances = collector
                .OfClass(typeof(ScheduleSheetInstance))
                .ToElements()
                .OfType<ScheduleSheetInstance>();

            foreach (var scheduleSheetInstance in scheduleSheetInstances)
            {
                var scheduleId = scheduleSheetInstance.ScheduleId;
                if (scheduleId == ElementId.InvalidElementId) continue;
                if (doc.GetElement(scheduleId) is ViewSchedule viewSchedule) yield return viewSchedule;
            }
        }
    }
    #endregion
}
