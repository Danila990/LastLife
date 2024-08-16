using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity;
using Core.Entity.InteractionLogic.Interactions;
using Core.Inventory.Items;
using Core.ResourcesSystem;
using Core.Services.SaveSystem.SaveAdapters.EntitySave;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Carry
{
	public abstract class CarriedContext : ItemContext, ISavableEntity
	{		
		[SerializeField] private ResourceType _resourceType;
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private CarriedItemPickUpInteraction _interaction;
		[SerializeField] private AudioClip _dropSound;
		[SerializeField]
		[HideIf("OnlyInteract")]
		public CarryArgs CarryArgs;
		public bool OnlyInteract;
		public string FactoryId { get; set; }
		public bool CanSave { get; set; } = true;

		public ResourceType ResourceType => _resourceType;
		public Rigidbody Rigidbody => _rigidbody;
		
		private ReactiveCommand<CarriedContext> _onStateChanged; 
		public IReactiveCommand<CarriedContext> OnStateChanged => _onStateChanged; 

		protected override void OnCreated(IObjectResolver resolver)
		{
			_onStateChanged = new ReactiveCommand<CarriedContext>();
			_onStateChanged.AddTo(this);
			
			resolver.Inject(_interaction);
			base.OnCreated(resolver);
		}

		public bool IsAttached { get; set; }
		
		
		public void OnAttach()
		{
			if (!IsAttached)
			{
				IsAttached = true;
				ChangePhysic(IsAttached);
				_onStateChanged?.Execute(this);
				OnAttachInternal();
			}
		}

		public void ChangePhysic(bool isAttached)
		{
			_rigidbody.isKinematic = isAttached;
			_rigidbody.detectCollisions = !isAttached;
		}
		
		public void OnDetach()
		{
			if (IsAttached)
			{
				IsAttached = false;
				OnDetachInternal();
				ChangePhysic(IsAttached);
				_onStateChanged?.Execute(this);
				
				if (_dropSound )
				{
					LucidAudio
						.PlaySE(_dropSound)
						.SetPosition(transform.position)
						.SetSpatialBlend(1);
				}
			}
		}
		
		protected virtual void OnAttachInternal(){}
		protected virtual void OnDetachInternal(){}


		
	}
	
	[Serializable]
	public struct CarryArgs
	{
		public Transform LPoint;
		public Transform RPoint;
		public Vector3 Offset;
		public float HintHeight;
	}
}
