using System;
using System.Collections.Generic;
using Core.Entity.Characters;
using Core.Equipment.Data;
using Core.Equipment.Impl;
using UniRx;

namespace Core.Equipment.Inventory
{
	public class ActiveEquipment : IDisposable
	{
		#region Fields
		private readonly CharacterContext _owner;
		private readonly AllEquipment _allEquipment;
		private readonly EquipmentInventory _inventory;
		
		private readonly UnequipHandler _unequipHandler;
		private readonly EquipHandler _equipHandler;
		
		private readonly Dictionary<EquipmentPartType, EquipmentEntityContext> _storage;

		private readonly ReactiveCommand<EquipmentEntityContext> _onEquip;
		private readonly ReactiveCommand<UnequipArgs> _onUnequip;

		private readonly Action<EquipmentEntityContext> _onRemoveCallback;
		private readonly Func<IEquipmentArgs, EquipmentEntityContext> _onSelectCallback;

		private bool _isDisposed;
		#endregion
		
		#region Props
		public IEnumerable<EquipmentEntityContext> Items => _storage.Values;
		public IReadOnlyDictionary<EquipmentPartType, EquipmentEntityContext> Equipment => _storage;

		public IReactiveCommand<EquipmentEntityContext> OnEquip => _isDisposed ? null : _onEquip;
		public IReactiveCommand<UnequipArgs> OnUnequip => _isDisposed ? null : _onUnequip;

		#endregion

		public ActiveEquipment(
			AllEquipment allEquipment,
			CharacterContext owner,
			EquipmentInventory inventory,
			Func<IEquipmentArgs, EquipmentEntityContext> onSelectCallback,
			Action<EquipmentEntityContext> onRemoveCallback
		)
		{
			_owner = owner;
			_inventory = inventory;
			_allEquipment = allEquipment;
			_onSelectCallback = onSelectCallback;
			_onRemoveCallback = onRemoveCallback;

			_unequipHandler = new UnequipHandler(this);
			_equipHandler = new EquipHandler(this);
			_onEquip = new ReactiveCommand<EquipmentEntityContext>();
			_onUnequip = new ReactiveCommand<UnequipArgs>();
			_storage = new Dictionary<EquipmentPartType, EquipmentEntityContext>();
		}
		
		public void Dispose()
		{
			_onEquip?.Dispose();
			_onUnequip?.Dispose();
			_isDisposed = true;
		}
		
		public bool Select(EquipmentPartType type, string id)
		{
			if (_isDisposed)
				return false;
			
			if (!_allEquipment.HasEquipment(type, id))
				return false;
			
			_equipHandler.Select(type, id);
			
			return true;
		}

		public bool Deselect(ref UnequipArgs unequipArgs)
		{
			if (_isDisposed)
				return false;
			
			if (_storage.TryGetValue(unequipArgs.PartType, out var part) && part != null)
			{
				unequipArgs.Context = part;
				_unequipHandler.Deselect(ref unequipArgs);
				return true;
			}

			return false;
		}

		public bool TryGetActivePart<TContext>(EquipmentPartType type, out TContext part) where TContext : EquipmentEntityContext
		{
			part = default;
			if (_storage.TryGetValue(type, out var context))
			{
				part = context as TContext;
				return context is TContext;
			}

			return false;
		}
		
		private void SwitchPart(EquipmentEntityContext context)
		{
			var unequipArgs = new UnequipArgs(UnequipReason.BySelf, context.PartType);
			Deselect(ref unequipArgs);
				
			_storage[context.PartType] = context;
			context.OnEquip(_owner, _inventory);
			_onEquip?.Execute(context);
		}
		
		private class UnequipHandler
		{
			private readonly ActiveEquipment _activeEquipment;

			public UnequipHandler(ActiveEquipment activeEquipment)
			{
				_activeEquipment = activeEquipment;
			}

			public void Deselect(ref UnequipArgs unequipArgs)
			{
				switch (unequipArgs.Reason)
				{
					case UnequipReason.ByPlayer:
						DeselectByDefault(ref unequipArgs);
						break;
					case UnequipReason.BySelf:
						DeselectByDefault(ref unequipArgs);
						break;
					case UnequipReason.ByDestroy:
						DeselectByDestroy(ref unequipArgs);
						break;
					default:
						DeselectByDefault(ref unequipArgs);
						break;
				}

			}

			private void DeselectByDefault(ref UnequipArgs unequipArgs)
			{
				unequipArgs.Context.OnUnequip();
				_activeEquipment._storage.Remove(unequipArgs.PartType);
				_activeEquipment._onUnequip?.Execute(unequipArgs);
				_activeEquipment._onRemoveCallback(unequipArgs.Context);
			}
			
			private void DeselectByDestroy(ref UnequipArgs unequipArgs)
			{
				var key = (unequipArgs.Context.PartType, unequipArgs.Context.GetItemArgs().Id);
				var runtimeArgs = _activeEquipment._allEquipment.EquipmentByTypeId[key];
				if (runtimeArgs.IsUnlocked)
				{
					runtimeArgs.IsBlocked = true;
					_activeEquipment._allEquipment.ChangePart(runtimeArgs);
				}
				else
				{
					_activeEquipment._allEquipment.RemovePart(runtimeArgs.EquipmentArgs.PartType, runtimeArgs.EquipmentArgs.Id);
				}
				DeselectByDefault(ref unequipArgs);
			}
		}

		private class EquipHandler
		{
			private readonly ActiveEquipment _activeEquipment;

			public EquipHandler(ActiveEquipment activeEquipment)
			{
				_activeEquipment = activeEquipment;
			}

			public bool Select(EquipmentPartType type, string id)
			{
				var selected = _activeEquipment._allEquipment.EquipmentByTypeId[(type, id)];
				
				if (selected.IsBlocked)
					return false;
				
				_activeEquipment.SwitchPart(_activeEquipment._onSelectCallback(selected.EquipmentArgs));
				return true;
			}
		}
	}
}
