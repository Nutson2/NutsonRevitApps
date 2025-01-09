using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NutsonApp
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ProxyCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements
        )
        {
            var calledCommand = NutsonBaseExternalApplication.CalledCommand;
            var commands = NutsonBaseExternalApplication.NutsonExternalCommands;

            return commands.TryGetValue(calledCommand, out var curCommand)
                ? curCommand.Execute(commandData, ref message, elements)
                : Result.Failed;
        }

        private static string GetCalledCommand(string journalPath)
        {
            string commandName = string.Empty;
            var fileInfo = new FileInfo(journalPath);
            using (
                var st = new StreamReader(
                    fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                )
            )
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
