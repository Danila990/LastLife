using System;
using System.Threading;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters;
using Core.Inventory.Items.Weapon;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{

	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(ShootingAction), fileName = "ShootingAction")]
	public class ShootingAction : GenericEntityAction<ProjectileWeaponContext>
	{
		[Inject] private readonly ICameraService _cameraService;
		protected CharacterContext CharacterContext;
		//TODO: REMOVE FROM THERE
		private bool _isWaiting;

		public override void OnDeselect()
		{
			if (CurrentContext)
			{
				CurrentContext.SetShootStatus(false);
				Settings[CurrentContext.Uid].LastInput = false;
			}
		}
		
		public override void OnInput(bool state)
		{
			Settings[CurrentContext.Uid].LastInput = state;
		}
		
		protected async virtual UniTaskVoid WaitAiming(Action callback, CancellationToken token)
		{
			if (_isWaiting)
				return;
			_isWaiting = true;
			if (_cameraService.IsThirdPerson)
			{
				await CharacterContext.CurrentAdapter.AimController.IsAiming.WaitUntilValueChangedAsyncUniTask(cancellationToken: token);
			}
			_isWaiting = false;
			if (Settings[CurrentContext.Uid].LastInput)
			{
				callback();
			}
		}

		public override void OnInputUp()
		{
			CurrentContext.SetShootStatus(false);
		}
		
		public override void OnInputDown()
		{
			if (CharacterContext.CurrentAdapter.AimController.IsAiming.Value || !_cameraService.IsThirdPerson)
			{
				CurrentContext.SetShootStatus(true);
				return;
			}
			
			WaitAiming(() => CurrentContext.SetShootStatus(true), CharacterContext.destroyCancellationToken).Forget();
		}

		public override void SetContext(EntityContext context)
		{
			base.SetContext(context);
			CharacterContext = CurrentContext.Owner as CharacterContext;
			_isWaiting = false;
		}
	}
}