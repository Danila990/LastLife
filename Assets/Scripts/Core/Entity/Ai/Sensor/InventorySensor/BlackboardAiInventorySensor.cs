using System.Collections.Generic;
using Core.Entity.Ai.AiItem;
using Core.Entity.Ai.AiItem.Data;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items;
using NodeCanvas.Framework;
using Unity.Collections;
using UnityEngine;
using VContainer;

namespace Core.Entity.Ai.Sensor.InventorySensor
{
	public class BlackboardAiInventorySensor : AbstractInventorySensor
	{
		[SerializeField] private Blackboard _bb;
		[SerializeField] private string _parameterName;
		[SerializeField] private AiItemContextedData[] _validItems;
		[SerializeField] private GenericAiItemData[] _preloadedItems;
		private Variable<List<IAiItem>> _aiItemsVariable;
		private IEntityAdapter _entityAdapter;
		
		[Inject] private readonly IObjectResolver _resolver;
		
		protected override void OnInit()
		{
			_aiItemsVariable = _bb.GetVariable<List<IAiItem>>(_parameterName);
			_aiItemsVariable.value ??= new List<IAiItem>();
			foreach (var preloaded in _preloadedItems)
			{
				var item = preloaded.CreateAiItem(_entityAdapter.Entity);
				_resolver.Inject(item);
				_aiItemsVariable.value.Add(item);
			}
		}
		
		public void SetAdapter(IEntityAdapter aiHeadAdapter)
		{
			_entityAdapter = aiHeadAdapter;
		}
		
		public void DisableItem(IAiItemData itemData)
		{
			foreach (var aiItem in _aiItemsVariable.value)
			{
				if (aiItem.AiItemData == itemData)
				{
					aiItem.Disable();
					return;
				}
			}
		}

		protected override void OnItemRemoved(ItemContext obj)
		{
			_aiItemsVariable.value.RemoveSwapBack(item => item.ItemUid == obj.Uid);
		}
		
		protected override void OnItemAdded(ItemContext obj)
		{
			CreateAiItemFromContext(obj);
		}

		private void OnDisable()
		{
			foreach (var item in _aiItemsVariable.value)
			{
				item?.Dispose();
			}
		}

		private void CreateAiItemFromContext(ItemContext item)
		{
			foreach (var aiItemData in _validItems)
			{
				if (aiItemData.IsApplicable(item))
				{
					var aiItem = aiItemData.CreateAiItem(item, _entityAdapter.Entity);
					_aiItemsVariable.value.Add(aiItem);
					return;
				}
			}
		}

	}
}