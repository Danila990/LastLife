using System;
using Core.Inventory.Items.Weapon;
using LitMotion;
using TMPro;
using UniRx;
using Utils;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.ReloadUI
{
	public class ReloadingView : UiView
	{
		//public TextMeshProUGUI Clip;
	}
	
	public class ReloadUIController : UiController<ReloadingView>, IDisposable
	{
		private CompositeDisposable _clipWasting;
		private MotionHandle _handle;
		private TextMeshProUGUI _ammoInfo;

		public void SetClip(ProjectileWeaponContext weapon, TextMeshProUGUI ammoInfo)
		{
			_ammoInfo = ammoInfo;
			_ammoInfo.gameObject.SetActive(true);
			_clipWasting?.Dispose();
			_clipWasting = new CompositeDisposable();
			weapon.OnClipChanged.SubscribeWithState(weapon, OnClipAmountChanged).AddTo(_clipWasting);
			weapon.CurrentClip.SubscribeWithState(weapon, OnClipAmountChanged).AddTo(_clipWasting);
		}
		
		public void HideAll()
		{
			HideReloading();
			HideClip();
		}
		
		private void HideClip()
		{
			if(!_ammoInfo)
				return;
			
			_ammoInfo.text = "0/0";
			_ammoInfo.gameObject.SetActive(false);
			_ammoInfo = null;
		}

		private void OnClipAmountChanged(int currentClip, ProjectileWeaponContext projectileWeaponContext)
		{
			if(!_ammoInfo)
				return;
			
			_ammoInfo.text = $"{currentClip}/{projectileWeaponContext.ClipSize}";
		}
		
		public void ShowReloading(float duration)
		{
			TryCancelTween();
		}
		
		public void HideReloading()
		{
			TryCancelTween();
		}

		private void TryCancelTween()
		{
			_handle.IsActiveCancel();
		}
		
		public void Dispose()
		{
			TryCancelTween();
			_clipWasting?.Dispose();
		}
	}
}
