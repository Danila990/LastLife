using Core.Actions;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Inventory.Items
{
	public enum ItemIKGripType
	{
		None,
		OneBone,
		TwoBones
	}

	public class ItemContext : EntityContext, IItemActionsProvider
	{
		[CanBeNull]
		[field:SerializeField] 
		[field:Space(5)]
		public EntityContext Owner { get; set; }
		
		[field:ReadOnly, ShowInInspector] 
		public string ItemId { get; set; }
		
		[field:ReadOnly, ShowInInspector]
		public bool IsEnabled { get; private set; } = true;
		
		[TitleGroup("MainSettings")] 
		public ItemAnimator ItemAnimator;
		
		[TitleGroup("MainSettings")] 
		public bool CanAim;
		
		[TitleGroup("MainSettings")] 
		[SerializeField] 
		[InlineProperty]
		[BoxGroup("MainSettings/ItemActionProvider")]
		[HideLabel]
		protected ItemActionProvider ItemActionProvider;
		
		[field:TitleGroup("MainSettings")]
		[field:SerializeField]
		public bool HasQuantity { get; private set; }
		
		[ShowIf("HasQuantity")]
		[SerializeField] 
		[TitleGroup("MainSettings")]
		protected int _maxQuantity;
		public Sprite InvIco;

		[Inject] public readonly ICameraService CameraService;

		private readonly ReactiveProperty<int> _currentQuantity = new();
		public IReactiveProperty<int> CurrentQuantity => _currentQuantity;
	
		public IActionProvider ActionProvider => ItemActionProvider;
		public bool IsTpvMode => CameraService.IsThirdPerson;
		public bool IsPlayerOwned { get; private set; }
		public bool Savable { get; set; }
		protected BaseCharacterAdapter Adapter { get; private set; }

		protected override void OnCreated(IObjectResolver resolver)
		{
			ItemActionProvider.Initialize();
			
			if (Owner is CharacterContext characterContext)
			{
				OnAdapterSet(characterContext.CurrentAdapter);
			}
			
			_currentQuantity.AddTo(destroyCancellationToken);
		}
		
		public void OnAdapterSet(IEntityAdapter adapter)
		{
			if (ItemAnimator)
				ItemAnimator.OnAdapterSet(adapter);
			
			if (adapter is BaseCharacterAdapter characterAdapter)
				Adapter = characterAdapter;

			if (Adapter is PlayerCharacterAdapter playerCharacterAdapter)
				IsPlayerOwned = true;
		}
		
		public void SetEnabled(bool isEnabled)
		{
			IsEnabled = isEnabled;
		}

		public virtual void Refill(int quantity)
		{
			_currentQuantity.Value = Mathf.Min(_currentQuantity.Value + quantity, _maxQuantity);
		}
		
		public virtual void ItemInit(IOriginProxy inventory)
		{
			
		}

		public virtual void OnDeselect()
		{
#if UNITY_EDITOR
			if (ItemActionProvider?.EntityActions == null)
			{
				Debug.LogError($"Null Action provider in {this.GetType().Name}");
				return;
			}
#endif
			
			foreach (var entityAction in ItemActionProvider.EntityActions.Values)
			{
                entityAction.EntityAction.SetContext(this);
				entityAction.EntityAction.OnDeselect();
			}
			
			TryAimState(false);
			UnEquip();
		}
		
		protected virtual void UnEquip()
		{
			//CameraService.FpvCam.FPVHands.gameObject.SetActive(false);
			if (ItemAnimator)
				ItemAnimator.UnEquipModel();
		}

		public virtual void OnChangeView()
		{
			UnEquip();
			EquipModel();
		}

		public virtual void OnSelect()
		{
			EquipModel();
			foreach (var entityAction in ItemActionProvider.EntityActions.Values)
			{
				entityAction.EntityAction.SetContext(this);
			}
		}
		
		protected virtual void EquipModel()
		{
			if (ItemAnimator)
				ItemAnimator.EquipModel();
		}

		public virtual void OnInput(ActionKey actionKey, bool pressed)
		{
			if (!ItemActionProvider.EntityActions.TryGetValue(actionKey, out var action))
				return;
			
			action.EntityAction.OnInput(pressed);
		}
		
		public virtual void InputDown(ActionKey actionKey)
		{
			if (!ItemActionProvider.EntityActions.TryGetValue(actionKey, out var action))
				return;
			action.EntityAction.OnInputDown();
			
			if (Adapter is not PlayerCharacterAdapter) 
				return;
			
			if (Adapter.AimController.CurrState == AimState.Default)
			{
				TryAimState(true);
			}
		}
		
		public void InputUp(ActionKey actionKey)
		{
			if (!ItemActionProvider.EntityActions.TryGetValue(actionKey, out var action))
				return;

			action.EntityAction.OnInputUp();
		}
		
		public void TryAimState(bool state)
		{
			if (ItemAnimator)
				ItemAnimator.TrySetAimState(state && CanAim);
		}

		public virtual void InventoryUpdate()
		{
			if (ItemAnimator)
				ItemAnimator.OnUpdate(Time.deltaTime);
		}
	}

	public struct IKBindUpdater
	{
		public Transform IKTarget;
		public Transform IKBindTarget;
		public Vector3 PosOffset;
		public Quaternion RotOffset;

		public void UpdateBind()
		{
			IKTarget.position = IKBindTarget.position - PosOffset;
			IKTarget.rotation = IKBindTarget.rotation * RotOffset;
		}
	}
	
	public interface IItemActionsProvider
	{
		IActionProvider ActionProvider { get; }
		IReactiveProperty<int> CurrentQuantity { get; }
		bool HasQuantity { get; }
	}
}