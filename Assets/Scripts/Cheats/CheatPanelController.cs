using System;
using System.Collections.Generic;
using Cheats.CheatPanel;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Core.Services.Experience;
using Cysharp.Threading.Tasks;
using MessagePipe;
using RemoteServer;
using Ticket;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Cheats
{
	public class CheatPanelController : IInitializable, ITickable
	{
		private const int TAP_COUNT_LAUNCH = 10;
		private const float TAPS_DELTA = 1f;

		private int _tapCount;
		private float _prevTapTimestamp;
		
		private readonly CheatPanelView _cheatPanel;
		private readonly Button _cheatButton;
		private readonly IResourcesService _resources;
		private readonly IEnumerable<ICheatCommandProvider> _cheatCommandProviders;
		private readonly IRemoteAccess _remoteAccess;
		private readonly ITicketService _ticketService;
		private readonly IPublisher<ExperienceMessage> _experiencePublisher;
		private readonly List<CheatCommand> _cheatList = new List<CheatCommand>();
		public const string KEY_PASSWORD = "CheatPassword";

		private bool _cacheAuthCheatAllowed = false;


		public CheatPanelController(
			IRemoteAccess remoteAccess,
			ITicketService ticketService,
			IPublisher<ExperienceMessage> experiencePublisher,
			IEnumerable<ICheatCommandProvider> cheatCommandProviders,
			CheatPanelView cheatPanelView,
			Button cheatButton,
			IResourcesService resources)
		{
			_cheatPanel = cheatPanelView;
			_cheatButton = cheatButton;
			_resources = resources;
			_cheatCommandProviders = cheatCommandProviders;
			_remoteAccess = remoteAccess;
			_ticketService = ticketService;
			_experiencePublisher = experiencePublisher;
		}
        
		public void Initialize()
		{
			_cheatPanel.PasswordConfirm.onClick.AddListener(ConfirmPassword);
			RegisterButtons();
		}

		public void Tick()
		{
			if (Input.touchCount > 0)
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Ended && 
				    touch.position.y > Screen.height * 0.80f &&
				    touch.position.x > Screen.width * 0.45f &&
				    touch.position.x < Screen.width * 0.65f )
				{
					float timeSincePreviousClick = Time.realtimeSinceStartup - _prevTapTimestamp;
					if (timeSincePreviousClick < TAPS_DELTA)
					{
						_tapCount++;
						if (_tapCount == TAP_COUNT_LAUNCH)
						{
							AsyncCheckAccess().Forget();
						}
					}
					else
					{
						_tapCount = 1;
					}

					_prevTapTimestamp = Time.realtimeSinceStartup;
				}
			}
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.Q))
			{
				//AsyncCheckAccess().Forget();
				ForceOpen();
			}
#endif
		}

#if UNITY_EDITOR
		private void ForceOpen()
		{
			_cheatPanel.gameObject.SetActive(!_cheatPanel.gameObject.activeSelf);
			_cheatPanel.PasswordPanel.gameObject.SetActive(false);
			_cheatPanel.CheatButtonsPanel.gameObject.SetActive(true);
		}
#endif

		private async UniTaskVoid AsyncCheckAccess()
		{
			if (_cacheAuthCheatAllowed)
			{
				_cheatPanel.gameObject.SetActive(true);
				OpenCheatsMenu(true);
				return;
			}
			var res = await _remoteAccess.CheckRemoteAccess();
			_cheatPanel.gameObject.SetActive(true);
			OpenCheatsMenu(res);
		}
        
		private void OpenCheatsMenu(bool status)
		{
			if (status)
			{
				_cacheAuthCheatAllowed = true;
			}
			_cheatPanel.PasswordPanel.gameObject.SetActive(!status);
			_cheatPanel.CheatButtonsPanel.gameObject.SetActive(status);
		}

		private void ConfirmPassword()
		{
			ConfirmPasswordAsync().Forget();
		}

		private async UniTaskVoid ConfirmPasswordAsync()
		{
			var pass = _cheatPanel.PasswordInput.text;
			var res = await _remoteAccess.RegisterRemoteAccess(pass);
			if (res)
			{
				AsyncCheckAccess().Forget();
				return;
			}
			_cheatPanel.PasswordPanel.gameObject.SetActive(false);
		}

		private void RegisterButtons()
		{
			foreach (var chp in _cheatCommandProviders)
			{
				RegisterButton(chp.Execute, chp.ButtonText, chp.IsToggle);
			}
			RegisterButton(ExpBoost,"+5K XP",false);
			RegisterButton(GetTickets,"get \n tickets",false);
		}

		private void GetTickets(bool val)
		{
			if (_ticketService.CurrentTicketsCount < 200)
			{
				_ticketService.OnPurchaseTickets(200 - _ticketService.CurrentTicketsCount);
			}
			if (_resources.GetCurrentResourceCount(ResourceType.GoldTicket) < 10)
			{
				_resources.AddResource(ResourceType.GoldTicket, 10, new ResourceEventMetaData());
			}
			
			if (_resources.GetCurrentResourceCount(ResourceType.Fuel) < 10)
			{
				_resources.AddResource(ResourceType.Fuel, 10, new ResourceEventMetaData());
			}
		}

		private void ExpBoost(bool val)
		{
			_experiencePublisher.Publish(new ExperienceMessage(true, 5000, true));
		}
		
		private void RegisterButton(Action<bool> command, string name, bool toggle = true)
		{
			var buttonObject = Object.Instantiate(_cheatButton, _cheatPanel.ButtonRoot);
			_cheatList.Add(new CheatCommand(command, name,buttonObject,toggle));
		}
	}
}