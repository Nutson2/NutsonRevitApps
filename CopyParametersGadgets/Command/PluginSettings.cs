using System;
using System.IO;

namespace CopyParametersGadgets.Command
{
    internal static class PluginSettings
    {

        public static string GetSettingFilePath(string CommandName)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsFilePath = Path.Combine(path, "NutsonApp", CommandName+"Settings.txt");

            if (!Directory.Exists(Path.GetDirectoryName(settingsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
            }

            return settingsFilePath;
        }
    }
}