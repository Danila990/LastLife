using System;
using System.Collections.Generic;
using Core.Equipment.Impl;
using Core.Equipment.Inventory;
using Core.Render;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Db.Render
{

	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(CharactersRendererSo), fileName = nameof(CharactersRendererSo))]
	public class CharactersRendererSo : SerializedScriptableObject, IRendererData
	{
		[SerializeField] private CharacterRendererData[] _renderers;
		[SerializeField] private EquipmentRendererData[] _equipmentRenderer;
		[SerializeField] private RendererHolder _rendererPrefab;

		public IEnumerable<CharacterRendererData> Renderers => _renderers;
		public IEnumerable<EquipmentRendererData> EquipmentRenderer => _equipmentRenderer;
		public RendererHolder RendererPrefab => _rendererPrefab;
	}

	public interface IRendererData
	{
		IEnumerable<CharacterRendererData> Renderers { get; }
		IEnumerable<EquipmentRendererData> EquipmentRenderer { get; }
		RendererHolder RendererPrefab { get; }
	}


	[Serializable]
	public struct CharacterRendererData
	{
		[ValueDropdown("@ObjectsData.GetCharacters()")]
		public string CharacterId;
		public CharacterRenderer RendererPrefab;
	}
	
	[Serializable]
	public struct EquipmentRendererData
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Equipment)")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		public string EntityId;
		public RenderEquipmentEntityContext RendererPrefab;
	}
}