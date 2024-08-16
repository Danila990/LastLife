using System.Collections.Generic;
using UnityEngine;

namespace FileManagerSystem
{
	public interface IDataStorage
	{
		public void Set(string key, string data);

		public string Get(string key);

		public IEnumerable<string> GetAllKeys();

		public void Delete(string key);
	}
	
	public class DirectoryStorage : IDataStorage
	{
		private readonly string _path;

		public DirectoryStorage(string pathToDirectory)
		{
			if (!System.IO.Directory.Exists(pathToDirectory))
				System.IO.Directory.CreateDirectory(pathToDirectory);

			_path = pathToDirectory;
		}

		public void Delete(string fileName) 
			=> System.IO.File.Delete(_path + fileName);

		public string Get(string fileName) 
			=> System.IO.File.Exists(_path + fileName) ? System.IO.File.ReadAllText(_path + fileName) : string.Empty;

		public IEnumerable<string> GetAllKeys()
		{
			var strings = System.IO.Directory.GetFiles(_path);

			for (var i = 0; i < strings.Length; i++)
				strings[i] = System.IO.Path.GetFileName(strings[i]);

			return strings;
		}

		public void Set(string fileName, string data)
		{
			if(!System.IO.File.Exists(_path + fileName))
			{
				var stream = System.IO.File.Create(_path + fileName);
				stream.Close();
			}

			if (string.IsNullOrEmpty(data)) return;

			System.IO.File.WriteAllText(_path + fileName, data);
		}
	}
}