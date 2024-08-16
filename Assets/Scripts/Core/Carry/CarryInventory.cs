using System;
using Core.AnimationRigging;
using Core.CameraSystem;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Utils;
using VContainer;

namespace Core.Carry
{
	public class CarryInventory : MonoBehaviour
	{
		[SerializeField] private Transform _parent;
		[SerializeField] private RigArgs _rigArgs;

		[Inject] private readonly ICarryInventoryService _carryInventoryService;
		[Inject] private readonly ICameraService _cameraService;

		private Vector3 _defaultRHintPos;
		private Vector3 _defaultLHintPos;
		
		private PlayerCharacterAdapter _adapter;
		private CharacterContext _owner;

		private ReactiveProperty<CarriedContext> _currentCarried;
		private ReactiveCommand<CarriedContext> _onDrop;
		private ReactiveCommand<CarriedContext> _onPickUp;

		public IReactiveProperty<CarriedContext> CurrentCarried => _currentCarried;
		public IReactiveCommand<CarriedContext> OnDrop => _onDrop;
		public IReactiveCommand<CarriedContext> OnPickUp => _onPickUp;
		
		public bool HasContext => _currentCarried is { HasValue: true } && _currentCarried.Value != null;

		public void Init(CharacterContext owner)
		{
			_owner = owner;
			_currentCarried = new ReactiveProperty<CarriedContext>().AddTo(this);
			_onDrop = new ReactiveCommand<CarriedContext>().AddTo(this);
			_onPickUp = new ReactiveCommand<CarriedContext>().AddTo(this);
		}
		
		public void OnAdapterSet(BaseCharacterAdapter adapter)
		{
			if (adapter is PlayerCharacterAdapter player)
			{
				_adapter = player;
				SetRefToFsm();
			}
		}

		private void SetRefToFsm()
		{
			_adapter.MovementStateMachine.ReusableData.CarryInventory = this;
		}

		public void Take(CarriedContext context)
		{
			if (context.OnlyInteract)
			{
				context.OnAttach();
				return;
			}
			
			if(HasContext)
				return;
			
			_currentCarried.Value = context;
			Attach();
		}

		[Button]
		public void Remove()
		{
			if(!_currentCarried.Value)
				return;
			
			Detach();
		}

		private void Attach()
		{
			_onPickUp.Execute(_currentCarried.Value);
			_cameraService.SetThirdPerson();
			var contextTransform = _currentCarried.Value.transform;
			contextTransform.SetParent(_parent);
			contextTransform.localEulerAngles = _parent.localEulerAngles;
			contextTransform.localPosition = Vector3.zero;
			contextTransform.localPosition = _currentCarried.Value.CarryArgs.Offset;
			
			_currentCarried.Value.OnAdapterSet(_adapter);
			_carryInventoryService.Attach(this, _owner.Health.OnDeath);
			_currentCarried.Value.OnAttach();
			
			_defaultRHintPos = _rigArgs.RHint.localPosition;
			_defaultLHintPos = _rigArgs.LHint.localPosition;

			var rHintPos = _defaultRHintPos;
			var lHintPos = _defaultLHintPos;
			lHintPos.y = _currentCarried.Value.CarryArgs.HintHeight;
			rHintPos.y = _currentCarried.Value.CarryArgs.HintHeight;
			_rigArgs.LHint.localPosition = lHintPos;
			_rigArgs.RHint.localPosition = rHintPos;
			
			SetRigWeight(1f);
		}

		private void Update()
		{
			if (!HasContext)
				return;
			
			_rigArgs.LPoint.position = _currentCarried.Value.CarryArgs.LPoint.position;
			_rigArgs.RPoint.position = _currentCarried.Value.CarryArgs.RPoint.position;
		}

		private void Detach()
		{
			if (!HasContext)
				return;
			_currentCarried.Value.OnDetach();
			_currentCarried.Value.transform.SetParent(null);
			_currentCarried.Value.OnAdapterSet(null);
			_onDrop.Execute(_currentCarried.Value);
			_currentCarried.Value = null;
			_carryInventoryService.Detach();

			_rigArgs.LHint.localPosition = _defaultLHintPos;
			_rigArgs.RHint.localPosition = _defaultRHintPos;
			
			SetRigWeight(0f);
		}

		private void SetRigWeight(float weight)
		{
			foreach (var rig in _rigArgs.Rigs)
				rig.weight = weight;
		}

		public void OnDeath()
		{
			Detach();
		}
		
		public void OnDestroyed()
		{
			Detach();
		}
		
		[Serializable]
		private struct RigArgs
		{
			public Rig[] Rigs;
			
			public Transform LPoint;
			public Transform RPoint;
			
			public Transform RHint;
			public Transform LHint;
		}
		
		#region UNITY_EDITOR
		#if UNITY_EDITOR

		[Button]
		public void FindRefs()
		{

			_rigArgs.Rigs = new Rig[2];
			_rigArgs.Rigs[0] = GameObjectUtils.FindObjectByName(gameObject, "Carrying_IKLRig").GetComponent<Rig>();
			_rigArgs.Rigs[1] = GameObjectUtils.FindObjectByName(gameObject, "Carrying_IKRRig").GetComponent<Rig>();
			
			
			_rigArgs.LHint = GameObjectUtils.FindObjectByName(gameObject, "HintR").transform;
			_rigArgs.RHint = GameObjectUtils.FindObjectByName(gameObject, "HintL").transform;
			_rigArgs.LPoint = GameObjectUtils.FindObjectByName(gameObject, "IKTargetL").transform;
			_rigArgs.RPoint = GameObjectUtils.FindObjectByName(gameObject, "IKTargetR").transform;
		}
		

		
		
		#endif
		#endregion

	}
}
