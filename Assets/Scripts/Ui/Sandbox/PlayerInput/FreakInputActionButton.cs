using System;
using ControlFreak2;
using Core.Actions;
using Core.Entity.Characters;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui.Sandbox.PlayerInput
{
	public class FreakInputActionButton : InputButtonElement
	{
		public Image IconImg;
		public TouchButton TouchButton;
		public Image BgImg;
		public GameObject LockGO;
		
		private IDisposable _disposable;
		private IDisposable _isUnlockedDisposable;
		private IDisposable _enableDisposable;
		private MotionHandle _handle;

		public override void OnContextChanged(CharacterContext characterContext)
		{
			
		}
		
		public override void SetUp(IEntityActionController entityActionData)
		{
			_disposable?.Dispose();
			_isUnlockedDisposable?.Dispose();
			_handle.IsActiveComplete();

			
			if (entityActionData.ActionData.Icon)
				IconImg.sprite = entityActionData.ActionData.Icon;

			if (entityActionData.EntityAction is IActionWithCooldown actionWithCooldown)
			{
				_disposable = actionWithCooldown.OnCooldown.Subscribe(OnCooldown);
			}
			
			if (entityActionData.EntityAction is IUnlockableAction { IsUnlocked: not null } unlockableAction)
			{
				_isUnlockedDisposable = unlockableAction.IsUnlocked.Subscribe(OnUnlocked);
				OnUnlocked(unlockableAction.IsUnlocked.Value);
			}
			else
			{
				OnUnlocked(true);
			}
			
			if (entityActionData.EntityAction is IEnableAction enableAction)
			{
				_enableDisposable = enableAction.IsEnabled.Subscribe(OnEnableChanged);
			}
			else
			{
				gameObject.SetActive(true);
			}
		}
		
		private void OnEnableChanged(bool obj)
		{
			gameObject.SetActive(obj);
		}

		private void OnUnlocked(bool isUnlocked)
		{
			IconImg.gameObject.SetActive(isUnlocked);
			TouchButton.ShowOrHideControl(isUnlocked, true);

			if (LockGO)
			{
				LockGO.SetActive(!isUnlocked);
			}
		}

		private void OnCooldown(float duration)
		{
			BlockAnimation(duration);
		}
		
		[Button]
		public void BlockAnimation(float duration)
		{
			BlockAsyncAnimation(duration);
		}

		protected virtual void BlockAsyncAnimation(float duration)
		{
			if (_handle.IsActive())
				return;


			BgImg.enabled = true;
			TouchButton.HideControl();
			var testColor = IconImg.color;
			testColor.a = 0.3f;
			IconImg.color = testColor;

			BgImg.fillAmount = 0f;
			_handle = LMotion
				.Create(0f, 1f, duration)
				.WithOnComplete(() =>
				{
					TouchButton.ShowControl();

					var color = IconImg.color;
					color.a = 1f;
					IconImg.color = color;

					BgImg.enabled = false;
				})
				.WithOnCancel(() =>
				{
					TouchButton.HideControl(false);
					var color = IconImg.color;
					color.a = 1f;
					IconImg.color = color;
					BgImg.enabled = false;
				})
				.BindToFillAmount(BgImg);
		}

		private void OnDestroy()
		{
			_disposable?.Dispose();
			_enableDisposable?.Dispose();
			_handle.IsActiveCancel();
		}

		public override void Hide()
		{
			TouchButton.HideControl(true);
			IconImg.gameObject.SetActive(false);
			
			_disposable?.Dispose();
			_enableDisposable?.Dispose();
			_isUnlockedDisposable?.Dispose();
			_handle.IsActiveCancel();
			
			if (LockGO)
			{
				LockGO.SetActive(false);
			}
		}
		
		public override void Show()
		{
			IconImg.gameObject.SetActive(true);
			TouchButton.ShowControl(true);
		}
		public override void Disable() => TouchButton.ResetControl();
	}
}