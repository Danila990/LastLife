using System;
using System.Collections.Generic;
using Core.Equipment.Impl;
using Core.Factory;
using Core.Render;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Equipment.Inventory
{
	[Serializable]
	public class ProxyEquipmentData
	{
		[TableList]
		[SerializeField] private ProxyEquipmentItem[] _items;
		private readonly Dictionary<string, ProxyEquipmentItem> _proxyEquipmentItems = new();

		private IEquipmentFactory _factory;
		private IRendererStorage _rendererStorage;

		public void Init(IEquipmentFactory factory, IRendererStorage rendererStorage)
		{
			_rendererStorage = rendererStorage;
			_factory = factory;
			foreach (var item in _items)
				_proxyEquipmentItems.Add(item.FactoryId, item);
		}

		public EquipmentEntityContext GetProxyContext(string factoryId)
		{
			var findProxy = _proxyEquipmentItems.TryGetValue(factoryId, out var item);
			if (findProxy && !item.UsePrefab)
				factoryId = item.ProxyFactoryId;
			
			if (_rendererStorage.EquipmentEntityRenderers.TryGetValue(factoryId, out var renderer))
				return _factory.CreateFromPrefab(renderer, factoryId);

			if (findProxy)
			{
				return item.UsePrefab 
					? _factory.CreateFromPrefab(item.ProxyContext, factoryId)
					: _factory.CreateObject(item.ProxyFactoryId);
			}
			
			return _factory.CreateObject(factoryId);
		}

		[Serializable]
		public struct ProxyEquipmentItem
		{
			public bool UsePrefab;
			[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Equipment)")]
			[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
			public string FactoryId;

			[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Equipment)")]
			[HideIf("UsePrefab")]
			public string ProxyFactoryId;
			[ShowIf("UsePrefab")]

			public EquipmentEntityContext ProxyContext;
		}
	}
}