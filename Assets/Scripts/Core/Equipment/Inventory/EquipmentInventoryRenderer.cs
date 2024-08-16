using System.Collections.Generic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.Equipment.Data;
using Core.Equipment.Impl;
using Core.Factory;
using Core.InputSystem;
using Core.Render;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Equipment.Inventory
{
	public class EquipmentInventoryRenderer : MonoBehaviour, IEquipmentInventory
	{
		[SerializeField] private ProxyEquipmentData _proxyEquipmentData;
		[field: SerializeField] public SkinnedMeshRenderer Renderer { get; private set; }
		[SerializeField, TableList] private EquipmentOrigins[] Origins;

		private IEquipmentFactory _factory;
		private IEntityRepository _entityRepository;
		private IEquipmentSaveAdapter _saveAdapter;
		private IPlayerSpawnService _playerSpawnService;
		
		private readonly Dictionary<EquipmentPartType, EquipmentEntityContext> _equipmentInstances = new();
		
		private ActiveEquipment _referenceActiveEquipment;
		private CompositeDisposable _disposable;
		private string _characterId;
		private AllEquipment _allEquipment;

		[Inject]
		private void Construct(
			IEquipmentFactory factory,
			IEntityRepository entityRepository,
			IEquipmentSaveAdapter saveAdapter, 
			IPlayerSpawnService playerSpawnService,
			IRendererStorage rendererStorage
			)
		{
			_factory = factory;
			_entityRepository = entityRepository;
			_saveAdapter = saveAdapter;
			_playerSpawnService = playerSpawnService;
			
			_proxyEquipmentData.Init(factory, rendererStorage);
		}

		public void DisplayFromSave(AllEquipment allEquipment, string characterId)
		{
			_characterId = characterId;
			_allEquipment = allEquipment;
			Display();
		}

		public void DisplayFromRuntime(AllEquipment allEquipment, ActiveEquipment activeEquipment)
		{
			_referenceActiveEquipment = activeEquipment;
			_allEquipment = allEquipment;
			
			_disposable?.Dispose();
			_disposable = new CompositeDisposable();
			_referenceActiveEquipment.OnEquip?.Subscribe(OnEquip).AddTo(_disposable);
			_referenceActiveEquipment.OnUnequip?.Subscribe(OnUnequip).AddTo(_disposable);
			
			Display();
		}

		public void Hide()
		{
			DestroyAll();
			_disposable?.Dispose();
			
			_characterId = string.Empty;
			_referenceActiveEquipment = null;
			_allEquipment = null;
		}
		
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
			parts = null;
			return false;
		}
		
		public bool TryGetController(out EquipmentInventoryController controller)
		{
			controller = null;
			return false;
		}

		private void Display()
		{
			if(_referenceActiveEquipment != null)
				CreateFromRuntime();
			else
				CreateFromSave();
		}

		private void CreateFromSave()
		{
			if (_saveAdapter.TryGetPresets(_characterId, out var data))
			{
				foreach (var kpv in data.Items)
				{
					var key = (kpv.Key, kpv.Value);
					if (!_allEquipment.EquipmentByTypeId.TryGetValue(key, out var args) || args.IsBlocked)
						continue;
					
					if(_equipmentInstances.ContainsKey(kpv.Key))
						continue;

					var instance = _proxyEquipmentData.GetProxyContext(kpv.Value);
					instance.ChangeCurrentArgs(args.EquipmentArgs);
					instance.OnEquip(null, this);
					_equipmentInstances.Add(instance.PartType, instance);
				}
			}
		}
		
		private void CreateFromRuntime()
		{
			foreach (var item in _referenceActiveEquipment.Items)
			{
				var key = (item.PartType, item.GetItemArgs().Id);
				if (!_allEquipment.EquipmentByTypeId.TryGetValue(key, out var args) || args.IsBlocked)
					continue;
				
				if(_equipmentInstances.ContainsKey(item.PartType))
					continue;
				
				var instance = _proxyEquipmentData.GetProxyContext(item.GetItemArgs().FactoryId);
				instance.ChangeCurrentArgs(args.EquipmentArgs);
				instance.OnEquip(null, this);
				_equipmentInstances.Add(instance.PartType, instance);
			}
		}
		
		private void OnEquip(EquipmentEntityContext contextRef)
		{
			AddPart(contextRef);
		}
		
		private void OnUnequip(UnequipArgs args)
		{
			DestroyPart(args.Context.PartType);
		}

		private void AddPart(EquipmentEntityContext contextRef)
		{
			if(_equipmentInstances.ContainsKey(contextRef.PartType))
				DestroyPart(contextRef.PartType);
			
			var factoryId = contextRef.GetItemArgs().FactoryId;
			var instance = _proxyEquipmentData.GetProxyContext(factoryId);
			instance.ChangeCurrentArgs(contextRef.GetItemArgs());
			instance.OnEquip(null, this);
			_equipmentInstances.Add(instance.PartType, instance);
		}
		
		private void DestroyPart(EquipmentPartType type)
		{
			if (_equipmentInstances.TryGetValue(type, out var context))
			{
				context.OnDestroyed(_entityRepository);
				Destroy(context.gameObject);
				_equipmentInstances.Remove(type);
			}
		}
		
		private void DestroyAll()
		{
			foreach (var context in _equipmentInstances.Values)
			{
				context.OnDestroyed(_entityRepository);
				Destroy(context.gameObject);
			}
			
			_equipmentInstances.Clear();
		}

		#region UNITY_EDITOR
		#if UNITY_EDITOR
		
		[Button]
		private void FindMesh(string rName = "Body")
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
		
		[Button]
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

		#endif
		#endregion
	}
}
