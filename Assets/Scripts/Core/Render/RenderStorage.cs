using System.Collections.Generic;
using Core.Equipment.Impl;
using Core.Equipment.Inventory;
using Db.Render;
using UnityEngine;
using VContainer.Unity;

namespace Core.Render
{
	public interface IRendererStorage
	{
		IReadOnlyDictionary<string, CharacterRenderer> CharacterRenderers { get; }
		IReadOnlyDictionary<string, RenderEquipmentEntityContext> EquipmentEntityRenderers { get; }
	}
	
	public class RendererStorage : IRendererStorage, IInitializable
	{
		private readonly IRendererData _rendererData;

		private readonly Dictionary<string, CharacterRenderer> _characterRenderers;
		private readonly Dictionary<string, RenderEquipmentEntityContext> _equipmentEntityRenderers;

		public IReadOnlyDictionary<string, CharacterRenderer> CharacterRenderers => _characterRenderers;
		public IReadOnlyDictionary<string, RenderEquipmentEntityContext> EquipmentEntityRenderers => _equipmentEntityRenderers;

		public RendererStorage(IRendererData rendererData)
		{
			_rendererData = rendererData;

			_characterRenderers = new();
			_equipmentEntityRenderers = new();
		}
		
		public void Initialize()
		{
			ParseData();
		}
		
		private void ParseData()
		{
			foreach (var renderer in _rendererData.Renderers)
			{
#if  UNITY_EDITOR
				if (_characterRenderers.ContainsKey(renderer.CharacterId))
				{
					Debug.LogError($"The {renderer.CharacterId} is already contains in Dictionary");
					return;
				}
#endif

				_characterRenderers.Add(renderer.CharacterId, renderer.RendererPrefab);
			}
			
			foreach (var renderer in _rendererData.EquipmentRenderer)
			{
#if  UNITY_EDITOR
				if (_equipmentEntityRenderers.ContainsKey(renderer.EntityId))
				{
					Debug.LogError($"The {renderer.EntityId} is already contains in Dictionary");
					return;
				}
#endif
				_equipmentEntityRenderers.Add(renderer.EntityId, renderer.RendererPrefab);
			}
		}
	}
}
