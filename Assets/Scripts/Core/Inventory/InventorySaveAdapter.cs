using System;
using System.Collections.Generic;
using Core.InputSystem;
using Core.Inventory.Items;
using Core.Services.SaveSystem;
using Newtonsoft.Json;
using UnityEngine;
using Yarn.Unity;

namespace Core.Inventory
{
	public interface IInventorySaveAdapter
	{
		public void Refresh(ItemContext itemContext, int quantity);
		public bool TryGetData(out ReadOnlySavedItems data);
	}
	
	public class InventorySaveAdapter : IAutoLoadAdapter, IInventorySaveAdapter
	{
		private readonly IPlayerSpawnService _spawnService;
		
		private SavedItems _savedItems;
		
		public bool CanSave => true;
		public string SaveKey => "BaseInventoryItems";

		public InventorySaveAdapter(IPlayerSpawnService spawnService)
		{
			_spawnService = spawnService;
		}
		
		public string CreateSave()
		{
			RefreshAll();
			try
			{
				return JsonConvert.SerializeObject(_savedItems);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(InventorySaveAdapter)}]" + e.Message);
				return string.Empty;
			}
		}
		
		public void LoadSave(string value)
		{
			try
			{
				_savedItems = JsonConvert.DeserializeObject<SavedItems>(value);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(InventorySaveAdapter)}]" + e.Message);
			}
		}

		public void Refresh(ItemContext itemContext, int quantity)
		{
			if(itemContext == null)
				return;

			if (!itemContext.Savable)
				return;
			
			_savedItems.Data ??= new();
			if (quantity <= 0)
			{
				if(_savedItems.Data.ContainsKey(itemContext.ItemId))
					_savedItems.Data.Remove(itemContext.ItemId);
				
				return;
			}
			
			_savedItems.Data[itemContext.ItemId] = quantity;
		}
		
		private void RefreshAll()
		{
			var context = _spawnService.PlayerCharacterAdapter.CurrentContext;
			if(!context)
				return;

			var items = context.Inventory.InventoryItems;
			if(items == null || items.Count == 0)
				return;

			_savedItems.Data ??= new();
			foreach (var item in items)
			{
				if(!item.ItemContext.Savable || !item.ItemContext.HasQuantity)
					continue;
				
				if (item.ItemContext.CurrentQuantity.Value <= 0)
				{
					if(_savedItems.Data.ContainsKey(item.ItemContext.ItemId))
						_savedItems.Data.Remove(item.ItemContext.ItemId);
					continue;
				}
				
				_savedItems.Data[item.ItemContext.ItemId] = item.ItemContext.CurrentQuantity.Value;
			}
		}

		public bool TryGetData(out ReadOnlySavedItems data)
		{
			data = default(ReadOnlySavedItems);
			if (_savedItems.Data == null || _savedItems.Data.Count == 0)
				return false;

			data = new ReadOnlySavedItems(_savedItems.Data);
			return true;
		}
		
		[Serializable]
		private struct SavedItems
		{
			public Dictionary<string, int> Data;
		}
	}
	
	
	public readonly struct ReadOnlySavedItems
	{
		public readonly IEnumerable<KeyValuePair<string, int>> Data;

		public ReadOnlySavedItems(IEnumerable<KeyValuePair<string, int>> data)
		{
			Data = data;
		}
	}
}
