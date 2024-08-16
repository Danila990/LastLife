using System;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic.Interactions;
using Core.Quests.Messages;
using Installer;
using UniRx;
using UnityEngine;
using VContainer;

namespace Market.OilRig
{
	public class RefinerBooster : MonoBehaviour, IInjectableTag
	{
		[SerializeField] private RefinerObject _refiner;
		[SerializeField] private GenericInteraction _interaction;
		[SerializeField] private float _boostedInterval;

		[Inject] private IRefinerBoostService _boostService;
		[Inject] private IQuestMessageSender _questMessageSender;

		private IDisposable _disposable;
		
		private void Start()
		{
			_interaction.Used.Subscribe(OnUsed).AddTo(this);
			_refiner.ProcessState.Subscribe(OnProcessStateChanged).AddTo(this);
		}

		private void OnProcessStateChanged(bool status)
		{
			if (status)
			{
				_interaction.Enable();
				return;
			}
			
			_interaction.Disable();
		}
		
		private void OnUsed(CharacterContext _)
		{
			if (_boostService.Boost(_refiner.Id, _boostedInterval, out var _))
			{
				_questMessageSender.SendSpeedUpRefiningMessage(string.Empty);
				_interaction.Disable();
			}
		}
	}
}
