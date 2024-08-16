using AnnulusGames.LucidTools.Audio;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Loot
{
	public abstract class LootEntity : EntityContext, IInteractorVisiter
	{
		[SerializeField] private OutlineHighLight _outline;
		[SerializeField] private AudioClip _pickUpSound;
		[SerializeField] private Transform _toRotate;
		[SerializeField] private Vector3 _velocity;
		
		private bool _interacted;
		
		protected override void OnCreated(IObjectResolver resolver)
		{
			base.OnCreated(resolver);
			if (_outline)
			{
				_outline.Init();
				_outline.Enable();
			}
		}

		public void Init()
		{
			Physics.Raycast(MainTransform.position + new Vector3(0, 1f), Vector3.down, out var hit, 50f, LayerMasks.Environment);
			MainTransform.position = hit.point;
		}
		
		private void Update()
		{
			if(!_toRotate)
				return;

			_toRotate.Rotate(_velocity);
		}

		public InteractionResultMeta Accept(TriggerInteraction interaction, ref InteractionCallMeta meta)
		{
			if (interaction is not CharacterTriggerInteraction charInteraction || !charInteraction.Adapter)
				return StaticInteractionResultMeta.Default;
			
			if (charInteraction.Adapter == null)
				return StaticInteractionResultMeta.Default;
			
			if(charInteraction.Adapter.CurrentContext == null)
				return StaticInteractionResultMeta.Default;
			
			InteractWithPlayer(charInteraction.Adapter.CurrentContext);
				
			return StaticInteractionResultMeta.Default;
		}

		private void InteractWithPlayer(CharacterContext context)
		{
			if(_interacted)
				return;

			LucidAudio.PlaySE(_pickUpSound)
				.SetPosition(transform.position)
				.SetSpatialBlend(1f)
				.SetVolume(1f);
			_interacted = true;
			OnInteractWithPlayer(context);
			OnDestroyed(EntityRepository);
			Destroy(gameObject, 0.01f);
		}
		
		protected virtual void OnInteractWithPlayer(CharacterContext context)
		{
			
		}

		private void OnTriggerEnter(Collider other)
		{
			if(_interacted)
				return;

			VisiterUtils.TriggerVisit(other, this);
		}
	}
}
