using System.Linq;
using System.Threading;
using Common.SpawnPoint;
using Core.InputSystem;
using Cysharp.Threading.Tasks;
using Dialogue.Services.Interfaces;
using Dialogue.Services.Modules.MerchantShop;
using Dialogue.Ui;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Services;
using SharedUtils;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Entity.Ai.Merchant
{
	public class ModifiersMerchant : MerchantEntityContext
	{
		private static readonly int Work = Animator.StringToHash("Work");
		private static readonly int EndWork = Animator.StringToHash("EndWork");
		private ShopDialogueModuleArgs _merchantModule;
		private CapsulePlaceObject _capsule;
		[Inject] private readonly IPlayerSpawnService _playerSpawnService;
		[Inject] private readonly InstallerCancellationToken _installerCancellation;
		[Inject] private readonly DialogueUiController _dialogueUiController;
		private Variable<bool> _useCustomAnim;

		private void OnEnable()
		{
			GetComponent<EventRouter>().onCustomEvent += OnCustomEvent;
		}

		private void Start()
		{
			_merchantModule = GetComponent<ShopDialogueModuleArgs>();
			_capsule = (CapsulePlaceObject)_merchantModule.MerchantPlaceInfo.Objects.FirstOrDefault(x => x is CapsulePlaceObject);
			Debug.Assert(_capsule);
			_useCustomAnim = Blackboard.GetVariable<bool>("UseCustomAnimation");
			_useCustomAnim.onValueChanged += OnUse;
		}
		
		private void OnUse(object obj)
		{
			if (obj is true)
			{
				TeleportPlayerAsync().Forget();
				_capsule.OpenDoorForce();
				transform.position = _capsule.TpTargetForMerchant.position;
				_capsule.CloseDoor(.25f);
			}
		}

		private void OnDisable()
		{
			GetComponent<EventRouter>().onCustomEvent -= OnCustomEvent;
		}
		
		private void OnCustomEvent(string evtName, IEventData data)
		{
			if (evtName == "StartWorkOnCapsule")
			{
				NpcAnimator.Animator.SetTrigger(Work);
			}
			
			if (evtName == "EndWorkOnCapsule")
			{
				PlayerAnimation(destroyCancellationToken).Forget();
			}
		}

		private async UniTaskVoid PlayerAnimation(CancellationToken token)
		{
			_capsule.WorkingEffect.Play();
			await UniTask.Delay(.5f.ToSec(), cancellationToken: token);
			_capsule.WorkingEffect.Stop(true);
			await UniTask.Delay(.5f.ToSec(), cancellationToken: token);

			_capsule.OpenDoor();
			await UniTask.Delay(.5f.ToSec(), cancellationToken: token);
			_capsule.EndWorkEffect.Play();
			
			await UniTask.Delay(.5f.ToSec(), cancellationToken: token);
			_playerSpawnService.PlayerCharacterAdapter.Rigidbody.isKinematic = false;
			_playerSpawnService.PlayerCharacterAdapter.Rigidbody.detectCollisions = true;
			await _playerSpawnService.PlayerCharacterAdapter.ManualMoveTo(_capsule.ExitPoint);
			NpcAnimator.Animator.SetTrigger(EndWork);
			await UniTask.Delay(0.2f.ToSec(), cancellationToken: token);
			_capsule.CloseDoor(0);
			MerchantShopModule.EnableControl();
			_useCustomAnim.SetValue(false);
			_capsule.EndWorkEffect.Stop();
			_playerSpawnService.PlayerCharacterAdapter.CurrentContext.SetImmortal(false);
			_dialogueUiController.CreatePopup("MODIFICATOR APPLIED", null);
		}
		
		private async UniTaskVoid TeleportPlayerAsync()
		{
			var fsm = _playerSpawnService.PlayerCharacterAdapter.MovementStateMachine;
			fsm.ReusableData.TeleportPosition = _capsule.MoveTargetForPlayer.position;
			var previousState = fsm.State;
			fsm.ChangeState(fsm.TeleportState);
			await UniTask.Delay(1f.ToSec(), cancellationToken: _installerCancellation.Token);
			fsm.TeleportState.ResetConstraints();
			fsm.ChangeState(previousState);
		}
	}
}