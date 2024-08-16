using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entity.Characters;
using Core.Equipment.Data;
using Core.Equipment.Impl;
using Core.Equipment.Inventory;
using Core.InputSystem;
using Core.Services.SaveSystem;
using MessagePipe;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer.Unity;

namespace Core.Equipment
{

	public interface IEquipmentSaveAdapter
	{
		public bool TryGetPresets(out EquipmentPreset presets);
		public bool TryGetPresets(string characterId, out EquipmentPreset equipment);
		public bool TryGetEquipment(out SavableEquipmentArgs[] equipment);
	}
	
	public class EquipmentSaveAdapter : IPostInitializable, IDisposable, IEquipmentSaveAdapter, IAutoLoadAdapter
	{
		private readonly IPlayerSpawnService _spawnService;
		private readonly ISubscriber<PlayerContextChangedMessage> _subscriber;

		private CompositeDisposable _equipObserving;
		private CompositeDisposable _selfLifeScope;

		private CharactersEquipment _currentEquipment;
		
		public bool CanSave => true;
		public string SaveKey => "CharactersEquipment";
		private string CurrentContextId => _spawnService.PlayerCharacterAdapter.ContextId;
		private CharacterContext CurrentContext => 
			!_spawnService.PlayerCharacterAdapter || !_spawnService.PlayerCharacterAdapter.CurrentContext 
				? null 
				: _spawnService.PlayerCharacterAdapter.CurrentContext;
		
		public EquipmentSaveAdapter(
			IPlayerSpawnService spawnService,
			ISubscriber<PlayerContextChangedMessage> subscriber)
		{
			_spawnService = spawnService;
			_subscriber = subscriber;
		}
		
		public void PostInitialize()
		{
			_selfLifeScope = new CompositeDisposable();
			_subscriber.Subscribe(OnContextChanged).AddTo(_selfLifeScope);
		}

		public string CreateSave()
		{
			RefreshParts();

			try
			{
				return JsonConvert.SerializeObject(_currentEquipment);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(EquipmentSaveAdapter)}]" + e.Message);
				return "";
			}
		}
		
		public void LoadSave(string value)
		{
			try
			{
				_currentEquipment = JsonConvert.DeserializeObject<CharactersEquipment>(value);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(EquipmentSaveAdapter)}]" + e.Message);
			}

			_currentEquipment ??= new CharactersEquipment();
			_currentEquipment.ActiveEquipmentPresets ??= new SerializedDictionary<string, EquipmentPreset>();
		}
		
		public void Dispose()
		{
			_equipObserving?.Dispose();
			_selfLifeScope?.Dispose();
		}

		public bool TryGetPresets(out EquipmentPreset preset)
		{
			preset = null;
			
			if (!CurrentContext)
				return false;
			
			return TryGetPresets(CurrentContextId, out preset);
		}
		
		public bool TryGetPresets(string characterId, out EquipmentPreset preset)
		{
			preset = null;

			if (string.IsNullOrEmpty(characterId))
				return false;
			
			if (_currentEquipment?.ActiveEquipmentPresets == null)
				return false;
			
			if (!_currentEquipment.ActiveEquipmentPresets.ContainsKey(characterId))
				return false;
			
			preset = _currentEquipment.ActiveEquipmentPresets[characterId];
			return preset is { Items: { Count: > 0 } };
		}
		
		public bool TryGetEquipment(out SavableEquipmentArgs[] equipment)
		{
			equipment = null;
			
			if (_currentEquipment?.Parts == null || _currentEquipment.Parts.Length == 0)
				return false;
			
			equipment = _currentEquipment.Parts;
			return equipment != null;
		}
		
		private void OnContextChanged(PlayerContextChangedMessage msg)
		{
			_equipObserving?.Dispose();
			RefreshParts();
			
			if (!msg.Created)
				return;

			var activeEquipment = msg.CharacterContext.EquipmentInventory.Controller.ActiveEquipment;
			_equipObserving = new CompositeDisposable();
			activeEquipment.OnEquip.Subscribe(OnEquip).AddTo(_equipObserving);
			activeEquipment.OnUnequip.Subscribe(OnUnequip).AddTo(_equipObserving);
		}
		
		private void OnEquip(EquipmentEntityContext context)
		{
			RefreshPreset(context);
		}
		
		private void OnUnequip(UnequipArgs args)
		{
			if (args.Reason is UnequipReason.ByPlayer or UnequipReason.BySelf)
			{
				RefreshParts();
				RefreshPreset(args.Context, true);
			}
		}


		private void RefreshPreset(EquipmentEntityContext context, bool remove = false)
		{
			_currentEquipment ??= new();
			_currentEquipment.ActiveEquipmentPresets ??= new();
			if (!_currentEquipment.ActiveEquipmentPresets.ContainsKey(CurrentContextId))
			{
				_currentEquipment.ActiveEquipmentPresets[CurrentContextId] = new();
				_currentEquipment.ActiveEquipmentPresets[CurrentContextId].Items = new();
			}
				
			var preset = _currentEquipment.ActiveEquipmentPresets[CurrentContextId];

			if (remove)
			{
				if (preset.Items.ContainsKey(context.PartType))
					preset.Items.Remove(context.PartType);
				return;
			}
			
			preset.Items[context.PartType] = context.GetItemArgs().Id;
		}

		private void RefreshParts()
		{
			if(!CurrentContext)
				return;
			
			_currentEquipment ??= new();
			_currentEquipment.Parts = CurrentContext.EquipmentInventory.Controller.AllEquipment.Items
				.Where(x => x.IsUnlocked)
				.Select(x => CreateArgs(x.EquipmentArgs))
				.ToArray();
		}

		public SavableEquipmentArgs CreateArgs(IEquipmentArgs args)
		{
			var savableArgs = new SavableEquipmentArgs
			{
				ItemId = args.Id,
				Type = args.PartType
			};
			if (args is JetPackItemArgs jetPackArgs)
				savableArgs.Fuel = jetPackArgs.Fuel;

			return savableArgs;
		}
		
		[Serializable]
		private class CharactersEquipment
		{
			public Dictionary<string, EquipmentPreset> ActiveEquipmentPresets;
			public SavableEquipmentArgs[] Parts;
		}
	}


	[Serializable]
	public class EquipmentPreset
	{
		public Dictionary<EquipmentPartType, string> Items;
	}
	

	[Serializable]
	public class SavableEquipmentArgs
	{
		public string ItemId;
		public EquipmentPartType Type;
		public float Fuel;
	}
}
