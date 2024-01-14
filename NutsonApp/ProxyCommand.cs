using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Linq;

namespace NutsonApp
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    public class ProxyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var journalPath = NutsonBaseExternalApplication.UIControlledRevit.ControlledApplication.RecordingJournalFilename;
            var calledCommand = GetCalledCommand(journalPath);
            if (string.IsNullOrEmpty(calledCommand)) return Result.Failed;

            IExternalCommand curCommand = default;
            if (NutsonBaseExternalApplication.NutsonExternalCommands.ContainsKey(calledCommand))
                NutsonBaseExternalApplication.NutsonExternalCommands.TryGetValue(calledCommand, out curCommand);
            if (curCommand == null) { return Result.Failed; }
            return curCommand.Execute(commandData, ref message, elements);
        }

        private static string GetCalledCommand(string journalPath)
        {
            string commandName = string.Empty;
            var fileInfo = new FileInfo(journalPath);
            using (var st = new StreamReader(fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                st.BaseStream.Seek(-200, SeekOrigin.End);
                while (!st.EndOfStream)
                {
                    var curString = st.ReadLine();
                    if (curString.StartsWith(" Jrn.RibbonEvent"))
                    {
                        commandName = curString.Split('%').Last().Split(':').First();
                        break;
                    }
                }
            }

            return commandName;
        }
    }

}
