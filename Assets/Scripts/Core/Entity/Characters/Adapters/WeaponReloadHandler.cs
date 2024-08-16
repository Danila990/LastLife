using System;
using Core.Inventory.Items.Weapon;
using SharedUtils;
using Ui.Sandbox.InventoryUi;
using Ui.Sandbox.ReloadUI;
using UniRx;

namespace Core.Entity.Characters.Adapters
{
	public class WeaponReloadHandler : IDisposable
	{
		private readonly ReloadUIController _uiController;
		private readonly InventoryPreviewController _inventoryPreviewController;

		private float _reloadDelay;
		
		private ProjectileWeaponContext _weapon;
		private IDisposable _reloadHandler;
		private IDisposable _clipHandler;
		private IDisposable _autoReloadHandler;

		public WeaponReloadHandler(ReloadUIController uiController, InventoryPreviewController inventoryPreviewController)
		{
			_uiController = uiController;
			_inventoryPreviewController = inventoryPreviewController;
		}

		public void SetWeapon(ProjectileWeaponContext weapon, float reloadDelay)
		{
			Reset();
			_reloadDelay = reloadDelay;
			_weapon = weapon;
			_reloadHandler = _weapon.Reloading.Subscribe(OnReload);
			_clipHandler = _weapon.CurrentClip.Subscribe(OnClipChanged);
			_uiController.SetClip(weapon, _inventoryPreviewController.Items[weapon.ItemId].AmmoInfo);
		}

		private void OnClipChanged(int clip)
		{
			if (clip == 0)
				ReloadWithDelay();
		}

		private void ReloadWithDelay()
		{
			_autoReloadHandler?.Dispose();
			_autoReloadHandler =
				Observable
					.Timer(_reloadDelay.ToSec())
					.Subscribe(_ => TryReload());
		}
		
		private void TryReload()
		{
			if(_weapon && _weapon.ShouldReload)
				_weapon.StartReload();
		}
			
		public void Reset()
		{
			_clipHandler?.Dispose();
			_reloadHandler?.Dispose();
			_autoReloadHandler?.Dispose();
			_autoReloadHandler = null;
			_reloadHandler = null;
			_clipHandler = null;

			_reloadDelay = 0f;
			
			_uiController.HideAll();
			_uiController.HideReloading();
		}

		public void Dispose()
		{
			_reloadHandler?.Dispose();
			_autoReloadHandler?.Dispose();
			_clipHandler?.Dispose();
		}
			
		private void OnReload(bool isReloading)
		{
			if (!isReloading)
			{
				_uiController.HideReloading();
				return;
			}
				
			_uiController.ShowReloading(_weapon.ReloadTime);
		}
	}
}
