using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;

namespace NutsonApp
{
    public class ExternalEventFileWatcher : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            NutsonBaseExternalApplication.LoadExternalCommand(
                    NutsonBaseExternalApplication.newAssemblyInFolder);
        }

        public string GetName()
        {
            return "File watcher";
        }
    }

}
