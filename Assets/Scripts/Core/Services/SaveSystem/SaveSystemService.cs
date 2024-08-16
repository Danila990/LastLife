using System;
using System.Collections.Generic;
using FileManagerSystem;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Core.Services.SaveSystem
{
	public class SaveSystemService : ISaveSystemService, IStartable, IDisposable
	{
		private readonly IEnumerable<ISavableAdapter> _saveSystemAdapters;
		private readonly ISceneLoaderService _sceneLoaderService;
		private readonly string _saveDataMeta;
		private readonly FileManager _fileManager = new FileManager();
		private Dictionary<string, string> _savableDictionary = new Dictionary<string, string>();
		private IDisposable _disposable;

		public SaveSystemService(
			IEnumerable<ISavableAdapter> saveSystemAdapters,
			ISceneLoaderService sceneLoaderService,
			string saveDataMeta)
		{
			_saveSystemAdapters = saveSystemAdapters;
			_sceneLoaderService = sceneLoaderService;
			_saveDataMeta = saveDataMeta;
		}

		public void Start()
		{
			_disposable = _sceneLoaderService.BeforeSceneChange.Subscribe(BeforeSceneChange);
			Application.focusChanged += ApplicationOnfocusChanged; 
			Load();
		}
		
		public bool TryGetFromLoadedStorage(string key, out string result) => _savableDictionary.TryGetValue(key, out result);

		private void Load()
		{
			var loaded = _fileManager.GetData<Dictionary<string, string>>(_saveDataMeta);
			if (loaded is null)
				return;
			_savableDictionary = loaded;
			NotifyAutoLoadedAdapters();
		}

		private void NotifyAutoLoadedAdapters()
		{
			foreach (var adapter in _saveSystemAdapters)
			{
				if (adapter is IAutoLoadAdapter autoLoadAdapter)
				{
					if (_savableDictionary.TryGetValue(autoLoadAdapter.SaveKey, out var save))
					{
						autoLoadAdapter.LoadSave(save);
					}
				}
			}
		}
		
		private void ApplicationOnfocusChanged(bool inFocus)
		{
			if (!inFocus)
			{
				Save();
			}
		}
		
		private void BeforeSceneChange(SceneChangeEventData data)
		{
			Save();
		}

		private void Save()
		{
			foreach (var savableService in _saveSystemAdapters)
			{
				if (!savableService.CanSave)
					continue;
				var save = savableService.CreateSave();
				_savableDictionary[savableService.SaveKey] = save;
			}
			
			_fileManager.SaveData(_saveDataMeta, _savableDictionary);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
			Application.focusChanged -= ApplicationOnfocusChanged; 
		}
	}
	
	public interface ISaveSystemService
	{
		bool TryGetFromLoadedStorage(string key, out string result);
	}

	public interface IAutoLoadAdapter : ILoadableAdapter
	{
		
	}
	
	public interface ILoadableAdapter : ISavableAdapter
	{
		void LoadSave(string value);
	}

	public interface ISavableAdapter
	{
		bool CanSave { get; }
		string SaveKey { get; }
		string CreateSave();
	}
}