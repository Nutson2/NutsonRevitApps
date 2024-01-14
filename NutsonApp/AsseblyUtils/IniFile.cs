// Decompiled with JetBrains decompiler
// Type: AddInManager.IniFile
// Assembly: AddInManager, Version=2015.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5BDD7A83-FA69-4D91-8F2B-9F16E915F05A
// Assembly location: C:\Users\Nutson\Downloads\Revit-AddInManager\AddInManager.dll

using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NutsonApp
{
    public class IniFile
    {
        private readonly string m_filePath;

        public string FilePath => m_filePath;

        public IniFile(string filePath)
        {
            m_filePath = filePath;
            if (File.Exists(m_filePath))
                return;
            FileUtils.CreateFile(m_filePath);
            FileUtils.SetWriteable(m_filePath);
        }

        public void WriteSection(string iniSection) => WritePrivateProfileSection(iniSection, null, m_filePath);

        public void Write(string iniSection, string iniKey, object iniValue) => WritePrivateProfileString(iniSection, iniKey, iniValue.ToString(), m_filePath);

        public string ReadString(string iniSection, string iniKey)
        {
            StringBuilder retVal = new StringBuilder(byte.MaxValue);
            GetPrivateProfileString(iniSection, iniKey, "", retVal, byte.MaxValue, m_filePath);
            return retVal.ToString();
        }

        public int ReadInt(string iniSection, string iniKey) => GetPrivateProfileInt(iniSection, iniKey, 0, m_filePath);

        [DllImport("kernel32.dll")]
        private static extern int WritePrivateProfileSection(
          string lpAppName,
          string lpString,
          string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern int WritePrivateProfileString(
          string section,
          string key,
          string val,
          string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileInt(
          string section,
          string key,
          int def,
          string filePath);

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(
          string section,
          string key,
          string defaultValue,
          StringBuilder retVal,
          int size,
          string filePath);
    }
}
