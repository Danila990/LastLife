using System;
using Adv.Services.Interfaces;
using AnnulusGames.LucidTools.Audio;
using SharedUtils;
using Ticket;
using Ui.Sandbox.WorldSpaceUI;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class CodeLockerInteraction : PlayerInputInteraction
	{
		[SerializeField] private Vector3 _offset;
		[SerializeField] private Color _correctColor = Color.green;
		[SerializeField] private Color _incorrectColor = Color.red;
		[SerializeField] private float _coloringDuration = 0.5f;
		[SerializeField] private AudioClip _errorSound;

		[SerializeField] private UnityEvent _onCorrectCode;
		[SerializeField] private UnityEvent _onIncorrectCode;
		
		[Inject] private readonly ITicketService _ticketService;
		[Inject] private readonly IAdvService _advService;
		
		private Color _defaultColor;
		private WorldButtonPresenter _defaultWorldButton;
		/*private TicketWorldButtonPresenter _ticketWorldButton;
		private AdvWorldButtonPresenter _advWorldButton;*/
		private WorldCodeLockerUI _currentUI;
		private IDisposable _coloring;

		private bool _isExecuted;
		
		protected override void OnStart()
		{
			_defaultWorldButton = new WorldButtonPresenter(Callback);
			/*_ticketWorldButton = new TicketWorldButtonPresenter(_ticketService, RewardCallback, "Open secret door in boss room");
			_advWorldButton = new AdvWorldButtonPresenter(_advService, RewardCallback, "Open secret door in boss room");*/
		}

		protected override void OnPlayerEnter()
		{
			if(_isExecuted)
				return;
			
			DisableUI();
			ShowUI();
		}

		protected override void OnPlayerExit()
		{
			DisableUI();
		}

		private void ShowUI()
		{
			_currentUI = WorldSpaceUIService.GetUI<WorldCodeLockerUI>(_worldButtonKey);
			_defaultWorldButton.Attach(_currentUI.ActivateButton);
			/*_advWorldButton.Attach(_currentUI.AdvButton);
			_ticketWorldButton.Attach(_currentUI.TicketButton);*/
			_defaultColor = _currentUI.ColoredImage.color;
			_currentUI.Target = transform;
			_currentUI.Offset = _offset;
		}
        
		private void DisableUI()
		{
			if (!_currentUI)
				return;
            
			_currentUI.ColoredImage.color = _defaultColor;
			_defaultWorldButton?.Dispose();
			/*_advWorldButton?.Dispose();
			_ticketWorldButton?.Dispose();*/
			_currentUI.IsInactive = true;
			_currentUI = null;
		}

		public override void Callback()
		{
			if (IsCorrect(_currentUI.ResultStr))
			{
				OnCorrectCode();
				return;
			}
            
			OnIncorrectCode();
		}

		private void RewardCallback()
		{
			OnCorrectCode();
			DisableUI();
		}

		protected virtual void OnCorrectCode()
		{
			ColorImage(_correctColor);
			_isExecuted = true;
			_onCorrectCode?.Invoke();
		}
        
		protected virtual void OnIncorrectCode()
		{
			LucidAudio
				.PlaySE(_errorSound)
				.SetPosition(transform.position)
				.SetSpatialBlend(1);
			
			ColorImage(_incorrectColor);
			_onIncorrectCode?.Invoke();
		}

		private void ColorImage(Color color)
		{
			_coloring?.Dispose();
			_currentUI.ColoredImage.color = color;
            
			_coloring = Observable
				.Timer(_coloringDuration.ToSec())
				.TakeWhile(_ => _currentUI != null)
				.Subscribe(_ =>
				{
					_currentUI.ColoredImage.color = _defaultColor;
				});

		}
		private bool IsCorrect(string code) => false;

	}
}
