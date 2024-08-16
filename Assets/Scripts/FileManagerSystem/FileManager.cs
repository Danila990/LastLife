using System.IO;
using UnityEditor;
using UnityEngine;

namespace FileManagerSystem
{
	public class FileManager
	{
		private static readonly string SavePath
#if UNITY_EDITOR
			= $"{Application.dataPath.Replace("/Assets", string.Empty)}{SAVE_FOLDER}/";
#else
			= $"{Application.persistentDataPath}{SAVE_FOLDER}/";
#endif
		private readonly IDataStorage _storage = new DirectoryStorage(SavePath);
		private readonly JsonDataProducer _dataProducer = new JsonDataProducer(VERSION_PATCH, VERSION);
		private const string SAVE_FOLDER = "/Save";
		private const string VERSION = "0.1";
		private const string VERSION_PATCH = "&VERSION={0}&";

#if UNITY_EDITOR
		[MenuItem("Tools/ClearSaves _%#H")]
		public static void ClearSave()
		{
			PlayerPrefs.DeleteAll();
			if (!Directory.Exists(SavePath))
			{
				Debug.Log($"No Directory {SavePath}");
				return;
			}
			
			var files = Directory.GetFiles(SavePath);
			if (files.Length == 0)
			{
				Debug.Log($"No Saves");
				return;
			}
			foreach (var f in files)
			{
				Debug.Log($"[DELETED] {f}");
				File.Delete(f);
			}
		}
#endif
	
		public void SaveData<T>(string fileName, T data)
		{
			var preparedData = _dataProducer.Serialize(data);
			_storage.Set(fileName, preparedData);
		}
		
		public T GetData<T>(string fileName)
		{
			return _dataProducer.Deserialize<T>(_storage.Get(fileName));
		}
	}
}