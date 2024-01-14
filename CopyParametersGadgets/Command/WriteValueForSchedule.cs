using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using CopyParametersGadgets.WriteValueForScheduleCommand;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CopyParametersGadgets.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class WriteValueForSchedule: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            var vm   = new VMUserInput(commandData.Application.ActiveUIDocument);
            var view = new UserInput(vm);

            view.ShowDialog();

            return Result.Succeeded;
        }
    }
}
