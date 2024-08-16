using System;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic.Interactions;
using Installer;
using UniRx;
using UnityEngine;
using VContainer;

namespace MiniGames
{
	public class InteractionMiniGameStarter : MonoBehaviour, IInjectableTag
	{
		[SerializeField] private GenericInteraction _interaction;

		[SerializeField] private string _miniGameName;
		
		[Inject] private readonly IMiniGameService _miniGameService;
		public event Action<bool> OnMiniGameCompleted;
		public event Action<CharacterContext> OnMiniGameStarted;
		public GenericInteraction Interaction => _interaction;
		
		private void Start()
		{
			_interaction.Used.Subscribe(OnClick).AddTo(this);
		}
		
		private void OnClick(CharacterContext context)
		{
			_miniGameService.PlayMiniGame(_miniGameName, Callback, context);
			OnMiniGameStarted?.Invoke(context);
		}
		
		private void Callback(bool status)
		{
			OnMiniGameCompleted?.Invoke(status);
		}
	}
}