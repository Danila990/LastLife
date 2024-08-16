using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entity.Characters;
using Core.InputSystem;
using Core.Inventory.Items.Weapon;
using Core.Services;
using Db.ObjectData;
using MessagePipe;
using Newtonsoft.Json;
using SharedUtils.PlayerPrefs;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Core.Actions.SpecialAbilities
{
	public interface IAbilitiesControllerService
	{
		/// <summary>
		/// Character Id -> Ablitity Id
		/// </summary>
		IReadOnlyDictionary<string, string> CurrentAbilityPair { get; }
		IReadOnlyDictionary<string, string> AdditionalAbilityPair { get; }
		/// <summary>
		/// Ability Id -> AbilityController
		/// </summary>
		IReadOnlyDictionary<string, AbilityController> AbilityControllers { get; }
		
		void ConnectAbilityToPlayer(CharacterContext characterContext, string abilityId, bool additional);
		void SaveAbilityConnection(string characterId, string abilityId);
	}
	
	public class AbilitiesControllerService : IInitializable, IDisposable, IAbilitiesControllerService
	{
		private readonly IItemStorage _itemStorage;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IScriptableActionsData _actionsData;
		private readonly ISubscriber<PlayerContextChangedMessage> _contextChangedMessage;
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly SortedList<string, AbilityController> _abilityControllers = new SortedList<string, AbilityController>();
		private readonly SortedList<string, string> _currentAbilityPair = new SortedList<string, string>();
		private readonly SortedList<string, string> _additionalAbilities = new SortedList<string, string>();
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		
		private AbilityPairs _overrideAbilityPairs;
		
		public IReadOnlyDictionary<string, string> CurrentAbilityPair => _currentAbilityPair;
		public IReadOnlyDictionary<string, string> AdditionalAbilityPair => _additionalAbilities;
		
		public IReadOnlyDictionary<string, AbilityController> AbilityControllers => _abilityControllers;
		
		public AbilitiesControllerService(
			IItemStorage itemStorage,
			IItemUnlockService itemUnlockService,
			IScriptableActionsData actionsData, 
			ISubscriber<PlayerContextChangedMessage> contextChangedMessage,
			IPlayerSpawnService playerSpawnService,
			IPlayerPrefsManager playerPrefsManager)
		{
			_itemStorage = itemStorage;
			_itemUnlockService = itemUnlockService;
			_actionsData = actionsData;
			_contextChangedMessage = contextChangedMessage;
			_playerSpawnService = playerSpawnService;
			_playerPrefsManager = playerPrefsManager;
		}

		public void Initialize()
		{
			InitializeAbilities();
			InitActionPairs();
			_contextChangedMessage.Subscribe(OnPlayerSpawned, message => message.Created).AddTo(_compositeDisposable);
			_itemUnlockService.OnItemUnlock.Subscribe(OnUnlocked).AddTo(_compositeDisposable);
		}
		
		private void OnUnlocked(ObjectData obj)
		{
			if (_abilityControllers.TryGetValue(obj.Id, out var abilityController))
			{
				abilityController.SetIsUnlocked(true);
			}
		}

		private void OnPlayerSpawned(PlayerContextChangedMessage obj)
		{
			
			if (_currentAbilityPair.TryGetValue(_playerSpawnService.ActiveCharacterData.Id, out var abilityId))
				ConnectAbilityToPlayer(obj.CharacterContext, abilityId, false);
			
			if (_additionalAbilities.TryGetValue(_playerSpawnService.ActiveCharacterData.Id, out abilityId))
				ConnectAbilityToPlayer(obj.CharacterContext, abilityId, true);
		}
		
		public void ConnectAbilityToPlayer(CharacterContext characterContext, string abilityId, bool additionalAbility)
		{
			var weapon = characterContext
				.Inventory
				.InventoryItems
				.First(pair => pair.ItemContext is ProjectileWeaponContext)
				.ItemContext;

			var hasAbility = _abilityControllers.TryGetValue(abilityId, out var connectedAbility);
			Debug.Assert(hasAbility);
			
			var actionController = connectedAbility.GetEntityActionController();
			weapon.ActionProvider.AddAbility(actionController, additionalAbility);
			
			if (characterContext.Inventory.SelectedItem == weapon)
			{
				actionController.EntityAction.SetContext(weapon);
				characterContext.Inventory.ForceRefresh();
			}
		}
		
		public void SaveAbilityConnection(string characterId, string abilityId)
		{
			var abilityPairOverride = new AbilityPair
			{
				CharacterId = characterId,
				AbilityId = abilityId,
			};
			
			PlaceAbilityToArray(ref abilityPairOverride);
			Debug.Assert(_overrideAbilityPairs.AbilityPair != null);
			SaveOverrides(_overrideAbilityPairs);
		}
		
		private void PlaceAbilityToArray(ref AbilityPair abilityPairOverride)
		{
			if (_overrideAbilityPairs.AbilityPair == null)
			{
				_overrideAbilityPairs.AbilityPair = new[]
				{
					abilityPairOverride
				};
				return;
			}

			var findOverride = Array.IndexOf(_overrideAbilityPairs.AbilityPair, abilityPairOverride);
			if (findOverride >= 0)
			{
				_overrideAbilityPairs.AbilityPair[findOverride] = abilityPairOverride;
			}
			else
			{
				var newCount = _overrideAbilityPairs.AbilityPair.Length + 1;
				Array.Resize(ref _overrideAbilityPairs.AbilityPair, newCount);
				_overrideAbilityPairs.AbilityPair[^1] = abilityPairOverride;
			}
		}

		private void SaveOverrides(in AbilityPairs abilityPairs)
		{
			try
			{
				var str = JsonConvert.SerializeObject(abilityPairs);
				_playerPrefsManager.SetValue<string>("AbilityPairsOverrides", str);

			}
			catch (Exception e)
			{
				Debug.LogError("AbilityControllers" + e.Message);
			}
			ApplyPairs(abilityPairs, _additionalAbilities);
		}

		private void InitActionPairs()
		{
			ApplyPairs(_actionsData.AbilityPairs, _currentAbilityPair);
			if (TryGetAbilityPairFromSaves(out _overrideAbilityPairs))
			{
				ApplyPairs(_overrideAbilityPairs, _additionalAbilities);
			}
		}

		private bool TryGetAbilityPairFromSaves(out AbilityPairs abilityPairs)
		{
			var str = _playerPrefsManager.GetValue<string>("AbilityPairsOverrides", "");
			if (string.IsNullOrEmpty(str))
			{
				abilityPairs = default;
				return false;
			}
			
			try
			{
				abilityPairs = JsonConvert.DeserializeObject<AbilityPairs>(str);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError("AbilityControllers" + e.Message);
			}
			
			abilityPairs = default;
			return false;
		}
		
		private static void ApplyPairs(in AbilityPairs abilityPairs, IDictionary<string, string> abilityPair)
		{
			foreach (var pair in abilityPairs.AbilityPair)
			{
				abilityPair[pair.CharacterId] = pair.AbilityId;
			}
		}

		private void InitializeAbilities()
		{
			foreach (var abilityObject in _itemStorage.AbilityObjects)
			{
				var ability = new AbilityController(abilityObject.Value);
				ability.SetIsUnlocked(_itemUnlockService.IsUnlocked(ability.Data));
				_abilityControllers.Add(abilityObject.Key, ability);
			}
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}