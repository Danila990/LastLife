using Core.Entity.Characters.Adapters;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.Entity.InteractionLogic.Interactions
{
	public sealed class PlayerTriggerVisiter : MonoBehaviour, IInteractorVisiter
	{
		private BoolReactiveProperty _interacted;
		private ReactiveCommand<PlayerTriggerInteractionData> _currentPlayer;

		[ShowInInspector]
		public bool Interacted => _interacted is not null && _interacted.Value;
		
		public IReadOnlyReactiveProperty<bool> InteractedProperty => _interacted;
		public IReactiveCommand<PlayerTriggerInteractionData> CurrentPlayer => _currentPlayer;
		
		public InteractionResultMeta Accept(TriggerInteraction interaction, ref InteractionCallMeta meta)
		{
			if (interaction is not CharacterTriggerInteraction charInteraction || !charInteraction.Adapter)
				return StaticInteractionResultMeta.Default;
			
			if (charInteraction.Adapter == null)
				return StaticInteractionResultMeta.Default;
			
			if(charInteraction.Adapter.CurrentContext == null)
				return StaticInteractionResultMeta.Default;

			if (_interacted.Value)
			{
				InteractExitWithPlayer(charInteraction);
			}
			else
			{
				InteractEnterWithPlayer(charInteraction);
			}
				
			return StaticInteractionResultMeta.Default;
		}

		private void OnEnable()
		{
			_interacted = new BoolReactiveProperty();
			_currentPlayer = new ReactiveCommand<PlayerTriggerInteractionData>();
		}
		
		private void OnDisable()
		{
			_interacted?.Dispose();
			_currentPlayer?.Dispose();
		}

		private void InteractEnterWithPlayer(CharacterTriggerInteraction adapterCurrentContext)
		{
			_interacted.Value = true;
			_currentPlayer.Execute(new PlayerTriggerInteractionData(true, adapterCurrentContext.Adapter));
			OnInteractEnterWithPlayer(adapterCurrentContext);
		}
		
		private void InteractExitWithPlayer(CharacterTriggerInteraction adapterCurrentContext)
		{
			_interacted.Value = false;
			_currentPlayer.Execute(new PlayerTriggerInteractionData(false, adapterCurrentContext.Adapter));
			OnInteractExitWithPlayer(adapterCurrentContext);
		}

		private void OnInteractEnterWithPlayer(CharacterTriggerInteraction adapterCurrentContext) { }
		private void OnInteractExitWithPlayer(CharacterTriggerInteraction adapterCurrentContext) { }

		private void OnTriggerEnter(Collider other)
		{
			if(_interacted.Value)
				return;

			VisiterUtils.TriggerVisit(other, this);
		}

		private void OnTriggerExit(Collider other)
		{
			if(!_interacted.Value)
				return;
			
			VisiterUtils.TriggerVisit(other, this);
		}
		
		public readonly struct PlayerTriggerInteractionData
		{
			public readonly bool IsEnter;
			public readonly PlayerCharacterAdapter PlayerCharacterAdapter;
			
			public PlayerTriggerInteractionData(bool isEnter, PlayerCharacterAdapter playerCharacterAdapter)
			{
				IsEnter = isEnter;
				PlayerCharacterAdapter = playerCharacterAdapter;
			}
		}
	}
}