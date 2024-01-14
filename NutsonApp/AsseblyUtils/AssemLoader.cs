
using NutsonApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NutsonApp
{
    public class AssemLoader:IDisposable
    {
        private readonly List<string> m_refedFolders;
        private readonly Dictionary<string, DateTime> m_copiedFiles;
        private string m_originalFolder;
        private string m_tempFolder;
        private static readonly string m_dotnetDir = Environment.GetEnvironmentVariable("windir")
            + "\\Microsoft.NET\\Framework\\v2.0.50727";
        public static string m_resolvedAssemPath = string.Empty;
        private string m_revitAPIAssemblyFullName;

        public string OriginalFolder
        {
            get => m_originalFolder;
            set => m_originalFolder = value;
        }

        public string TempFolder
        {
            get => m_tempFolder;
            set => m_tempFolder = value;
        }

        public AssemLoader()
        {
            m_tempFolder = string.Empty;
            m_refedFolders = new List<string>();
            m_copiedFiles = new Dictionary<string, DateTime>();
            HookAssemblyResolve();
        }

        public void CopyGeneratedFilesBack()
        {
            foreach (string file in Directory.GetFiles(m_tempFolder, "*.*", SearchOption.AllDirectories))
            {
                if (m_copiedFiles.ContainsKey(file))
                {
                    DateTime copiedFile = m_copiedFiles[file];
                    if (new FileInfo(file).LastWriteTime > copiedFile)
                    {
                        string destinationFilename = m_originalFolder + file.Remove(0, m_tempFolder.Length);
                        FileUtils.CopyFile(file, destinationFilename);
                    }
                }
                else
                {
                    string destinationFilename = m_originalFolder + file.Remove(0, m_tempFolder.Length);
                    FileUtils.CopyFile(file, destinationFilename);
                }
            }
        }

        public Assembly LoadAddinsFromTempFolder(string originalFilePath)
        {
            if (string.IsNullOrEmpty(originalFilePath) || originalFilePath.StartsWith("\\") || !File.Exists(originalFilePath))
                return null;

            m_originalFolder = Path.GetDirectoryName(originalFilePath);

            StringBuilder stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
            stringBuilder.Append("-Executing-");

            m_tempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
            Assembly assembly = CopyAndLoadAddin(originalFilePath, false);
            return null == assembly || !IsAPIReferenced(assembly) ? null : assembly;
        }
        private void HookAssemblyResolve() => AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

        private void UnhookAssemblyResolve() => AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);

        private Assembly CopyAndLoadAddin(string srcFilePath, bool onlyCopyRelated)
        {
            string filePath = string.Empty;
            if (!FileUtils.FileExistsInFolder(srcFilePath, m_tempFolder))
            {
                string directoryName = Path.GetDirectoryName(srcFilePath);
                if (!m_refedFolders.Contains(directoryName))
                    m_refedFolders.Add(directoryName);
                List<FileInfo> allCopiedFiles = new List<FileInfo>();
                filePath = FileUtils.CopyFileToFolder(srcFilePath, m_tempFolder, onlyCopyRelated, allCopiedFiles);
                if (string.IsNullOrEmpty(filePath))
                    return null;
                foreach (FileInfo fileInfo in allCopiedFiles)
                    m_copiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTime);
            }
            return LoadAddin(filePath);
        }

        private Assembly LoadAddin(string filePath)
        {
            try
            {
                Monitor.Enter(this);
                return Assembly.LoadFile(filePath);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            lock (this)
            {
                AssemblyName assemblyName = new AssemblyName(args.Name);
                string str1 = SearchAssemblyFileInTempFolder(args.Name);
                if (File.Exists(str1))
                    return LoadAddin(str1);
                string srcFilePath = SearchAssemblyFileInOriginalFolders(args.Name);
                if (string.IsNullOrEmpty(srcFilePath))
                {
                    string[] strArray = args.Name.Split(',');
                    string assemName = strArray[0];
                    if (strArray.Length > 1)
                    {
                        string str2 = strArray[2];
                        if (assemName.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) 
                            && !str2.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
                            assemName = assemName.Substring(0, assemName.Length - ".resources".Length);
                        string str3 = SearchAssemblyFileInTempFolder(assemName);
                        if (File.Exists(str3))
                            return LoadAddin(str3);
                        srcFilePath = SearchAssemblyFileInOriginalFolders(assemName);
                    }
                }
                return CopyAndLoadAddin(srcFilePath, true);
            }
        }

        private string SearchAssemblyFileInTempFolder(string assemName)
        {
            string[] strArray = new string[2] { ".dll", ".exe" };
            if (!assemName.Contains(',')) return string.Empty;
            string str1 = assemName.Substring(0, assemName.IndexOf(','));
            foreach (string str2 in strArray)
            {
                string path = m_tempFolder + "\\" + str1 + str2;
                if (File.Exists(path))
                    return path;
            }
            return string.Empty;
        }

        private string SearchAssemblyFileInOriginalFolders(string assemName)
        {
            string[] typeOfExtention = new string[2] { ".dll", ".exe" };
            if (!assemName.Contains(',')) return string.Empty;
            string assemblyName = assemName.Substring(0, assemName.IndexOf(','));
            foreach (string extention in typeOfExtention)
            {
                string path = m_dotnetDir + "\\" + assemblyName + extention;
                if (File.Exists(path))
                    return path;
            }
            foreach (string extention in typeOfExtention)
            {
                foreach (string refedFolder in m_refedFolders)
                {
                    string path = refedFolder + "\\" + assemblyName + extention;
                    if (File.Exists(path))
                        return path;
                }
            }
            try
            {
                string path = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)).Parent.FullName + "\\Regression\\_RegressionTools\\";
                if (Directory.Exists(path))
                {
                    foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        if (Path.GetFileNameWithoutExtension(file).Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                            return file;
                    }
                }
            }
            catch (Exception) { }

            int num = assemName.IndexOf("XMLSerializers", StringComparison.OrdinalIgnoreCase);
            if (num == -1)
                return (string)null;
            assemName = "System.XML" + assemName.Substring(num + "XMLSerializers".Length);
            return SearchAssemblyFileInOriginalFolders(assemName);
        }

        private bool IsAPIReferenced(Assembly assembly)
        {
            if (string.IsNullOrEmpty(m_revitAPIAssemblyFullName))
            {
                foreach (Assembly assembly1 in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (string.Compare(assembly1.GetName().Name, "RevitAPI", true) == 0)
                    {
                        m_revitAPIAssemblyFullName = assembly1.GetName().Name;
                        break;
                    }
                }
            }
            foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
            {
                if (m_revitAPIAssemblyFullName == referencedAssembly.Name)
                    return true;
            }
            return false;
        }

        public void Dispose()
        {
            UnhookAssemblyResolve();
        }
    }
}
