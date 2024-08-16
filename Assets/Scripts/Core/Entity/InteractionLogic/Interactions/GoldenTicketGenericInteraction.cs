using System;
using Core.Entity.Characters;
using Core.InputSystem;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Cysharp.Threading.Tasks;
using MessagePipe;
using SharedUtils.PlayerPrefs;
using Sirenix.OdinInspector;
using Ui.Sandbox.WorldSpaceUI;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class GoldenTicketGenericInteraction : GenericInteraction
	{
		[SerializeField] private string _singleWordMeta;
		[SerializeField] private int _amount;
		[BoxGroup("Saves")] [SerializeField]
		private bool _savable;
		[BoxGroup("Saves")] [SerializeField] [ShowIf("_savable")] [ValueDropdown("@UniqueMapObject.PrefKeys")]
		private string _saveId;
		
		private IResourcesService _resourcesService;
		private IPlayerPrefsManager _playerPrefsManager;
		private IDisposable _disposable;
		private IPlayerSpawnService _playerSpawnService;
		
		private GoldenTicketWorldButtonPresenter _ticketWorldButtonPresenter;

		[Inject]
		private void Construct(IResourcesService resourcesService, IPlayerPrefsManager playerPrefsManager, IPlayerSpawnService playerSpawnService)
		{
			_resourcesService = resourcesService;
			_playerPrefsManager = playerPrefsManager;
			_playerSpawnService = playerSpawnService;
		}
		
		protected override void OnStart()
		{
			base.OnStart();
			if (_savable && _playerPrefsManager.HasKey(_saveId))
			{
				_disposable = PlayerSpawnService.PlayerCharacterAdapter.ContextChanged.Subscribe(AwaitContext);
				OnUsed?.Execute(_playerSpawnService.PlayerCharacterAdapter.CurrentContext);
				return;
			}
			_ticketWorldButtonPresenter = new GoldenTicketWorldButtonPresenter(_resourcesService, Callback, _singleWordMeta, _amount);
		}

		protected override void ShowUI()
		{
			CurrentUI = WorldSpaceUIService.GetUI<WorldSpaceButton>(_worldButtonKey);
			_ticketWorldButtonPresenter.Attach(CurrentUI.Button);
			CurrentUI.Target = _targetTransform;
			CurrentUI.Offset = Offset;
			CurrentUI.Button.Button.interactable = _resourcesService.GetCurrentResourceCount(ResourceType.GoldTicket) >= _amount;
		}
        
		protected override void DisableUI()
		{
			if (!CurrentUI)
				return;
            
			_ticketWorldButtonPresenter?.Dispose();
			CurrentUI.IsInactive = true;
			CurrentUI = null;
		}

		private void AwaitContext(CharacterContext context)
		{
			_disposable?.Dispose();
			Callback();
		}
		
		protected override void Select(CharacterContext characterContext)
		{
			OnUsed?.Execute(characterContext);
			
			if (_savable && !_playerPrefsManager.HasKey(_saveId))
				_playerPrefsManager.SetValue(_saveId, 1);
		}
	}

}
