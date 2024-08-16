using System;
using System.Collections.Generic;
using System.Linq;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.Equipment.Data;
using Core.Equipment.Impl;
using Core.Factory;
using Core.Factory.DataObjects;
using Core.Quests.Messages;
using Core.Services;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using VContainer;
using Object = UnityEngine.Object;

namespace Core.Equipment.Inventory
{

	public class EquipmentInventory : MonoBehaviour, IEquipmentInventory
	{
		#region Fields

		[SerializeField] private AudioClip _putOnClip;
		[SerializeField] private AudioClip _takeOffClip;
		[field: SerializeField] public SkinnedMeshRenderer Renderer { get; private set; }

		[SerializeField, TableList] private EquipmentOrigins[] Origins;
		[SerializeField, TableList] private EquipmentArmoredParts[] ArmoredParts;

		[Inject] private readonly IEquipmentFactory _factory;
		[Inject] private readonly IEntityRepository _entityRepository;
		[Inject] private readonly IQuestMessageSender _questMessageSender;

		private CharacterContext _owner;
		private PlayerCharacterAdapter _adapter;

		#endregion

		#region Props

		public EquipmentInventoryController Controller { get; private set; }

		#endregion

		public void Init(CharacterContext owner, IObjectResolver resolver, AllEquipment allEquipment)
		{
			_owner = owner;
			Controller = new EquipmentInventoryController(OnSelectPart, OnRemovePart, owner, this, _entityRepository, allEquipment);
			resolver.Inject(Controller);
		}

		private void PlaySound(AudioClip clip)
		{
			if (!clip)
				return;

			LucidAudio
				.PlaySE(clip)
				.SetPosition(transform.position)
				.SetVolume(0.3f);
		}


		#region Handlers

		public void OnAdapterSet(BaseCharacterAdapter adapter)
		{
			if (adapter is PlayerCharacterAdapter player)
				Controller.SetAdapter(player);
		}

		private EquipmentEntityContext OnSelectPart(IEquipmentArgs args)
		{
			var instance = _factory.CreateObject(args.FactoryId, _owner.IsFat);
			instance.ChangeCurrentArgs(args);
			PlaySound(_putOnClip);
			_questMessageSender.SendTakeEquipmentMessage(args.Id);
			return instance;
		}

		private void OnRemovePart(EquipmentEntityContext context)
		{
			PlaySound(_takeOffClip);
			context.OnDestroyed(_entityRepository);
			Destroy(context.gameObject);
		}

		private void Update()
		{
			if (Controller?.ActiveEquipment?.Items == null)
				return;

			foreach (var item in Controller.ActiveEquipment.Items)
			{
				if (item == null)
					continue;

				item.Tick();
			}
		}

		public void OnDestroyed()
		{
			Controller?.OnDestroyed();
		}

		#endregion

		#region Getters

		public bool TryGetOrigin(EquipmentPartType type, out EquipmentOrigins origin)
		{
			foreach (var data in Origins)
			{
				if (data.Type == type)
				{
					origin = data;
					return true;
				}
			}

			origin = default;
			return false;
		}

		public bool TryGetParts(EquipmentPartType type, out CharacterPartArmored[] parts)
		{
			foreach (var data in ArmoredParts)
			{
				if (data.Type == type)
				{
					parts = data.CharacterParts;
					return true;
				}
			}

			parts = null;
			return false;
		}

		public bool TryGetController(out EquipmentInventoryController controller)
		{
			controller = Controller;
			return Controller != null;
		}

		#endregion

		#region UNITY_EDITOR

#if UNITY_EDITOR

		[SerializeField, BoxGroup("Editor"), BoxGroup("Editor/FindMesh")] private string rName;
		[Button, BoxGroup("Editor"), BoxGroup("Editor/FindMesh")]
		private void FindMesh()
		{
			var components = GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var component in components)
			{
				if (component.name.Contains(rName))
				{
					Renderer = component;
					return;
				}
			}

			Renderer = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		[Button, BoxGroup("Editor"), BoxGroup("Editor/FindOrigins")]
		private void FindOrigins()
		{
			var origins = new List<EquipmentOrigins>();

			var jetPack = new EquipmentOrigins(EquipmentPartType.JetPack, new PlacementData("FALLBACK", GameObjectUtils.FindObjectByName(gameObject, "Spine1").transform, new Vector3(0, 0, -0.06f)));
			origins.Add(jetPack);

			var body = new EquipmentOrigins(EquipmentPartType.Body, new PlacementData("FALLBACK", GameObjectUtils.FindObjectByName(gameObject, "Spine1").transform, new Vector3(0, 0, 0)));
			origins.Add(body);
			var hat = new EquipmentOrigins(EquipmentPartType.Hat, new PlacementData("FALLBACK", GameObjectUtils.FindObjectByName(gameObject, "Head").transform, new Vector3(0, 0.23f, 0.12f)));
			origins.Add(hat);
			var foot = new EquipmentOrigins(EquipmentPartType.Foot, new PlacementData("FALLBACK", GameObjectUtils.FindObjectByName(gameObject, "Hips").transform, new Vector3(0, 0, 0)));
			origins.Add(foot);

			Origins = origins.ToArray();
		}

		[Inject] private IItemStorage _itemStorage;
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Equipment)")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[SerializeField, BoxGroup("Editor"), BoxGroup("Editor/Action")] private string factoryId;
		[SerializeField, BoxGroup("Editor"), BoxGroup("Editor/Action")] private EquipmentPartType type;

		[Button, BoxGroup("Editor"), BoxGroup("Editor/Action")]
		private void Equip()
		{
			var item = _itemStorage.EquipmentByType[type].FirstOrDefault(x => x.Args.Id == factoryId);

			if (item == null)
			{
				Debug.Log($"Not found object with {factoryId}");
				return;
			}
			var args = item.Args.GetCopy();
			Controller.AllEquipment.AddPart(new RuntimeEquipmentArgs(false, true, args));
			Controller.ActiveEquipment.Select(args.PartType, args.Id);
		}
#endif

		#endregion

	}

	[Serializable]
	public struct EquipmentOrigins
	{
		public EquipmentPartType Type;

		public PlacementData[] PartPlacements;
		public PlacementData Fallback;

		public EquipmentOrigins(EquipmentPartType type, PlacementData fallback)
		{
			Type = type;
			PartPlacements = Array.Empty<PlacementData>();
			Fallback = fallback;
		}

		public PlacementData GetOffset(string factoryId)
		{
			var partOffset = PartPlacements.FirstOrDefault(x => x.FactoryId == factoryId);
			return string.IsNullOrEmpty(partOffset.FactoryId) ? Fallback : partOffset;
		}
	}

	[Serializable]
	public struct PlacementData
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Equipment)")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		public string FactoryId;
		public Transform Origin;
		public Vector3 Offset;
		public Vector3 Rotation;

		public PlacementData(string factoryId, Transform origin, Vector3 offset)
		{
			FactoryId = factoryId;
			Origin = origin;
			Offset = offset;
			Rotation = Vector3.zero;
#if UNITY_EDITOR
			_createdObj = null;
#endif
		}

		#if UNITY_EDITOR

		private EquipmentEntityContext _createdObj;

		[Button]
		private void Spawn()
		{
			Remove();
			var data = UnityEditor.AssetDatabase.LoadAssetAtPath<FactoryData>("Assets/Settings/Data/Factory/FactoryData.asset");
			var equipmentData = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentItemsData>("Assets/Settings/Data/Equipment/AllEquipment.asset");

			if (data == null || equipmentData == null)
				return;

			var id = FactoryId;
			var prefab = (EquipmentEntityContext)data.Objects.First(x => x.Key == id).Object;
			var args = equipmentData.FindArgs(id);

			if (prefab == null || args == null)
				return;

			_createdObj = Object.Instantiate(prefab);
			_createdObj.ChangeCurrentArgs(args);
			_createdObj.OnEquip(null, Origin.root.gameObject.GetComponent<IEquipmentInventory>());
		}

		[Button]
		private void WritePosAndRot()
		{
			if (!_createdObj)
				return;

			Offset = _createdObj.MainTransform.localPosition;
			Rotation = _createdObj.MainTransform.localEulerAngles;
		}

		[Button]
		private void Remove()
		{
			if (_createdObj == null)
				return;

			Object.DestroyImmediate(_createdObj.gameObject);
		}
		#endif
	}

	[Serializable]
	public struct EquipmentArmoredParts
	{
		public EquipmentPartType Type;
		public CharacterPartArmored[] CharacterParts;
	}

}
