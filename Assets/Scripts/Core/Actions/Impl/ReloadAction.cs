using System;
using Core.Entity;
using Core.Inventory.Items.Weapon;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(ReloadAction), fileName = nameof(ReloadAction))]

	public class ReloadAction : GenericEntityAction<ProjectileWeaponContext>, IActionWithCooldown
	{
		private ReactiveCommand<float> _onCooldown;
		public IObservable<float> OnCooldown => _onCooldown;

		public override void Initialize()
		{
			_onCooldown = new ReactiveCommand<float>();
		}

		public override void SetContext(EntityContext context)
		{
			base.SetContext(context);

			CurrentContext.Reloading.Subscribe(OnReload);
		}

		private void OnReload(bool isReloading)
		{
			if (isReloading)
				_onCooldown.Execute(CurrentContext.ReloadTime);
		}
		
		public override void Dispose()
		{
			_onCooldown?.Dispose();
		}
		
		public override void OnDeselect()
		{
			CurrentContext.StopReload();
		}

		public override void OnInput(bool state) { }
		public override void OnInputUp() { }
		
		public override void OnInputDown()
		{
			if (CurrentContext.ShouldReload)
			{
				CurrentContext.StartReload();
			}
		}
	}
}
