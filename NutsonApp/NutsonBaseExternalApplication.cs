using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;

namespace NutsonApp
{

    public class NutsonBaseExternalApplication : IExternalApplication
    {
        public static List<string>                         newAssemblyInFolder    = default;
        public static Dictionary<string, IExternalCommand> NutsonExternalCommands = default;
        public static string                               CurrentAssemblyFullName { get; set; }
        public static UIControlledApplication              UIControlledRevit      = default;
        private static RibbonBuilder                       ribbonBuilder          = default;

        const string ExternalCommandsFolder = "NutsonAppExternalCommand";
        static readonly string tabName = "Nutson";

        ExternalEvent ExternalEvent = null;
        FileSystemWatcher fileWatcher = default;
        public Result OnShutdown(UIControlledApplication application)
        {
            fileWatcher.Changed -= FileWatcher_Changed;
            fileWatcher.Deleted -= FileWatcher_Deleted;
            fileWatcher.Created -= FileWatcher_Created;

            return Result.Succeeded;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            UIControlledRevit      = application;
            NutsonExternalCommands = new Dictionary<string, IExternalCommand>();
            newAssemblyInFolder    = new List<string>();
            ribbonBuilder          = new RibbonBuilder(application, tabName);

            CurrentAssemblyFullName = Assembly.GetExecutingAssembly().Location;
            var exCmdPath = Path.GetDirectoryName(CurrentAssemblyFullName) + "\\" + ExternalCommandsFolder;
            if (!Directory.Exists(exCmdPath)) Directory.CreateDirectory(exCmdPath);

            #region FileSystemWatcher

            fileWatcher = new FileSystemWatcher(exCmdPath);
            fileWatcher.Changed += FileWatcher_Changed;
            fileWatcher.Deleted += FileWatcher_Deleted;
            fileWatcher.Created += FileWatcher_Created;
            fileWatcher.Filter = "*.dll";
            fileWatcher.EnableRaisingEvents = true;

            ExternalEventFileWatcher eventFileWatcher = new ExternalEventFileWatcher();
            ExternalEvent = ExternalEvent.Create(eventFileWatcher);

            #endregion

            LoadExternalCommand(Directory.GetFiles(exCmdPath)
                                        .Where(x => Path.GetExtension(x) == ".dll")
                                        .ToList());

            return Result.Succeeded;
        }

        public static void LoadExternalCommand(IList<string> AssemblyFileCollection)
        {
            if (AssemblyFileCollection == null || AssemblyFileCollection.Count == 0) return;

            using (AssemLoader al=new AssemLoader())
            {
                var proxyCommandType = typeof(ProxyCommand);
                try
                {
                    foreach (string assemblyFile in AssemblyFileCollection)
                    {
                        var loadedAsm = al.LoadAddinsFromTempFolder(assemblyFile);
                        if(loadedAsm==null) continue;
                        var asmRibbonSetting = loadedAsm.GetTypes().Where(x => x.Name == "RibbonSetting").FirstOrDefault();
                        if (asmRibbonSetting == null) continue;

                        var asmMethod = asmRibbonSetting.GetMethods().Where(x => x.Name == "AddCommandToRibbon").First();
                        asmMethod.Invoke(asmMethod, new object[] { ribbonBuilder, proxyCommandType });
               
                        var asmCommands = loadedAsm.GetTypes().Where(x=>x.GetInterfaces().Contains(typeof(IExternalCommand)));
                        foreach (var item in asmCommands)
                        {
                            if (!(loadedAsm.CreateInstance(item.FullName) is IExternalCommand command)) continue;
                            if (NutsonExternalCommands.ContainsKey(item.Name))
                                NutsonExternalCommands[item.Name] = command ;
                            else
                                NutsonExternalCommands.Add(item.Name, command );
                        }
                    }

                }
                finally
                {
                    AssemblyFileCollection.Clear();
                }

            }
        }

        #region Обработчики событий FileSystemWatcher
        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            FillNewAssemblyCollection(e);
            ExternalEvent.Raise();
        }
        private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            FillNewAssemblyCollection(e);
            ExternalEvent.Raise();
        }
        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FillNewAssemblyCollection(e);
            ExternalEvent.Raise();
        }
        private void FillNewAssemblyCollection(FileSystemEventArgs e)
        {
            //newAssemblyInFolder = null;
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                if(!newAssemblyInFolder.Contains(e.FullPath))
                    newAssemblyInFolder.Add(e.FullPath);

            }
        }
        #endregion
    }
    class ExternalCommandAttribute:Attribute
    {
        public string PanelName { get; }
        public string ButtonName { get; }
        public string ButtonTooltip { get; }
        public ExternalCommandAttribute(string panelName, string buttonName, string buttonTooltip)
        {
            PanelName = panelName;
            ButtonName = buttonName;
            ButtonTooltip = buttonTooltip;
        }
    }
}
