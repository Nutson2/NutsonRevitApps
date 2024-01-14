using System;
using System.Collections.Generic;
using System.IO;

namespace NutsonApp
{
    public static class FileUtils
    {
        private const string TempFolderName = "NutsonPlugin";

        public static DateTime GetModifyTime(string filePath) => File.GetLastWriteTime(filePath);

        public static string CreateTempFolder(string prefix)
        {
            DirectoryInfo tempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), TempFolderName));
            if (!tempDirectory.Exists) tempDirectory.Create();
            foreach (DirectoryInfo directory in tempDirectory.GetDirectories())
            {
                try
                {
                    Directory.Delete(directory.FullName, true);
                }
                catch (Exception){}
            }
            string currentDate = string.Format("{0:yyyyMMdd_HHmmss_ffff}", DateTime.Now);
            DirectoryInfo tempSubDirectory = new DirectoryInfo(Path.Combine(tempDirectory.FullName, 
                prefix + currentDate));
            tempSubDirectory.Create();
            return tempSubDirectory.FullName;
        }

        public static void SetWriteable(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            FileAttributes fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
            File.SetAttributes(fileName, fileAttributes);
        }

        public static bool SameFile(string file1, string file2) => 0 == string.Compare(file1.Trim(), file2.Trim(), true);

        public static bool CreateFile(string filePath)
        {
            if (File.Exists(filePath)) return true;
            try
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                using (new FileInfo(filePath).Create())
                    SetWriteable(filePath);
            }
            catch (Exception)
            {
                return false;
            }
            return File.Exists(filePath);
        }

        public static void DeleteFile(string fileName)
        {
            if (!File.Exists(fileName)) return;
            FileAttributes fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
            File.SetAttributes(fileName, fileAttributes);
            try
            {
                File.Delete(fileName);
            }
            catch (Exception)
            {
            }
        }

        public static bool FileExistsInFolder(string filePath, string destFolder) => File.Exists(Path.Combine(destFolder, Path.GetFileName(filePath)));

        public static string CopyFileToFolder(
          string sourceFilePath,
          string destFolder,
          bool onlyCopyRelated,
          List<FileInfo> allCopiedFiles)
        {
            if (!File.Exists(sourceFilePath)) return null;
            string sourceFolder = Path.GetDirectoryName(sourceFilePath);
            if (onlyCopyRelated)
            {
                string searchPattern = Path.GetFileNameWithoutExtension(sourceFilePath) + ".*";
                foreach (string file in Directory.GetFiles(sourceFolder, searchPattern, SearchOption.TopDirectoryOnly))
                {
                    string fileName = Path.GetFileName(file);
                    string str = Path.Combine(destFolder, fileName);
                    if (CopyFile(file, str))
                    {
                        FileInfo fileInfo = new FileInfo(str);
                        allCopiedFiles.Add(fileInfo);
                    }
                }
            }
            else
            {
                long folderSize = GetFolderSize(sourceFolder);
                if (folderSize > 50L)
                {
                    CopyDirectory(sourceFolder, destFolder, allCopiedFiles);
                }
                else
                    CopyDirectory(sourceFolder, destFolder, allCopiedFiles);
            }
            string path = Path.Combine(destFolder, Path.GetFileName(sourceFilePath));
            return File.Exists(path) ? path : null;
        }

        public static bool CopyFile(string sourceFilename, string destinationFilename)
        {
            if (!File.Exists(sourceFilename))
                return false;
            FileAttributes fileAttributes1 = File.GetAttributes(sourceFilename) & ~FileAttributes.ReadOnly;
            File.SetAttributes(sourceFilename, fileAttributes1);
            if (File.Exists(destinationFilename))
            {
                FileAttributes fileAttributes2 = File.GetAttributes(destinationFilename) & ~FileAttributes.ReadOnly;
                File.SetAttributes(destinationFilename, fileAttributes2);
                File.Delete(destinationFilename);
            }
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(destinationFilename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFilename));
                File.Copy(sourceFilename, destinationFilename, true);
            }
            catch (Exception)
            {
                return false;
            }
            return File.Exists(destinationFilename);
        }

        public static void CopyDirectory(
          string sourceDir,
          string desDir,
          List<FileInfo> allCopiedFiles)
        {
            try
            {
                foreach (string directory in Directory.GetDirectories(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    string str = directory.Replace(sourceDir, "");
                    string path = desDir + str;
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
                foreach (string file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    string str1 = file.Replace(sourceDir, "");
                    string str2 = desDir + str1;
                    if (!Directory.Exists(Path.GetDirectoryName(str2)))
                        Directory.CreateDirectory(Path.GetDirectoryName(str2));
                    if (FileUtils.CopyFile(file, str2))
                        allCopiedFiles.Add(new FileInfo(str2));
                }
            }
            catch (Exception)
            {
            }
        }

        public static long GetFolderSize(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            long num = 0;
            foreach (FileSystemInfo fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                if (fileSystemInfo is FileInfo info)
                    num += info.Length;
                else
                    num += GetFolderSize(fileSystemInfo.FullName);
            }
            return num / 1024L / 1024L;
        }
    }
}
