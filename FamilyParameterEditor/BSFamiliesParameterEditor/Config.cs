using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FamilyParameterEditor
{

	public class Config
	{
		private readonly string filePath;

		private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();

		public Config()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			UriBuilder uriBuilder = new UriBuilder(executingAssembly.CodeBase);
			string path = Uri.UnescapeDataString(uriBuilder.Path);
			string text = Path.GetDirectoryName(path) + "\\FamilyParameterEditor";
			if (!Directory.Exists(text))
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(text);
				directoryInfo.CreateSubdirectory("FamilyParameterEditor");
			}
			string name = executingAssembly.GetName().Name;
			filePath = text + "\\" + name + ".cfg";
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Close();
			}

			StreamReader streamReader = new StreamReader(filePath);
            string text2;
            while ((text2 = streamReader.ReadLine()) != null)
			{
				if (!dictionary.ContainsKey(text2))
				{
					dictionary.Add(text2, streamReader.ReadLine());
				}
			}
			streamReader.Close();
		}

		private void UpdateFile()
		{
			StreamWriter streamWriter = new StreamWriter(filePath);
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				streamWriter.WriteLine(item.Key);
				streamWriter.WriteLine(item.Value);
			}
			streamWriter.Close();
		}

		public void Write(string key, string value)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = value;
			}
			else
			{
				dictionary.Add(key, value);
			}
			UpdateFile();
		}

		public string Read(string key, string defaultValue)
		{
			if (dictionary.ContainsKey(key))
			{
				return dictionary[key];
			}
			return defaultValue;
		}
	}
}
