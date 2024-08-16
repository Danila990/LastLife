using Core.Actions;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public abstract class StaticItemContext : EntityContext, IItemActionsProvider
	{
		[field:TitleGroup("MainSettings"), SerializeField] public bool HasQuantity { get; private set; }
		[ShowIf("HasQuantity"), TitleGroup("MainSettings"), SerializeField] protected int _maxQuantity;
		[TitleGroup("MainSettings"), SerializeField] private ItemActionProvider _itemActionProvider;

		
		private readonly ReactiveProperty<int> _currentQuantity = new ReactiveProperty<int>();
		public IActionProvider ActionProvider => _itemActionProvider;
		public IReactiveProperty<int> CurrentQuantity => _currentQuantity;
		public PlayerCharacterAdapter CharacterAdapter { get; private set; }

		private void Start()
		{
			_itemActionProvider.Initialize();
		}

		public virtual void TickOnActive()
		{
			
		}
		
		public void Attach(PlayerCharacterAdapter adapter)
		{
			CharacterAdapter = adapter;
			foreach (var entityAction in _itemActionProvider.EntityActions.Values)
			{
				entityAction.EntityAction.SetContext(this);
			}
		} 
		
		public virtual void OnInput(ActionKey actionKey, bool pressed)
		{
			if (!_itemActionProvider.EntityActions.TryGetValue(actionKey, out var action))
				return;
			
			action.EntityAction.OnInput(pressed);
		}
		
		public virtual void InputDown(ActionKey actionKey)
		{
			if (!_itemActionProvider.EntityActions.TryGetValue(actionKey, out var action))
				return;
			
			action.EntityAction.OnInputDown();
			// if (CharacterAdapter.AimController.CurrState == AimState.Default)
			// {
			// 	TryAimState(true);
			// }
		}
		
		public void InputUp(ActionKey actionKey)
		{
			if (!_itemActionProvider.EntityActions.TryGetValue(actionKey, out var action))
				return;

			action.EntityAction.OnInputUp();
		}
		
		public void TryAimState(bool state)
		{
			CharacterAdapter.SetAimState(state ? AimState.Aim : AimState.Default);
		}

		public virtual void Refill(int quantity)
		{
			_currentQuantity.Value = Mathf.Min(_currentQuantity.Value + quantity, _maxQuantity);
		}
	}

}