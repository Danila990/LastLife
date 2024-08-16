using System;
using Core.Entity.EntityUpgrade;
using Db.ObjectData;
using Ui.Widget;
using UniRx;
using UnityEngine;

namespace Ui.Sandbox.CharacterMenu
{
	public class CharacterSelectPresenter : IDisposable
	{
		private readonly CharacterMenuController _characterMenuController;
		private readonly CharacterObjectData _characterObject;
		private readonly Sprite _lockSprite;
		private readonly IDisposable _disposable;
		private readonly OutlineButtonWidgetWithNumber _charSelectButton;
		private bool _iapInWindowVersion;

		public CharacterObjectData ObjectData => _characterObject;
		public bool IsUnlocked { get; private set; }

		public CharacterSelectPresenter(
			CharacterMenuController characterMenuController, 
			OutlineButtonWidgetWithNumber charSelectButton,
			CharacterObjectData characterObject,
			Sprite lockSprite)
		{
			_charSelectButton = charSelectButton;
			_characterMenuController = characterMenuController;
			_characterObject = characterObject;
			_lockSprite = lockSprite;
			charSelectButton.IconImg.sprite = characterObject.Ico;
			
			_disposable = charSelectButton.Button
				.OnClickAsObservable()
				.Subscribe(OnClick);
		}

		public void OnConfigUpdated(bool iapInWindowVersion)
		{
			_iapInWindowVersion = iapInWindowVersion;
			SetUnlockStatus(IsUnlocked);
		}

		public void SetUnlockStatus(bool status)
		{
			IsUnlocked = status;
			if (_iapInWindowVersion && status == false)
			{
				_charSelectButton.gameObject.SetActive(true);
				_charSelectButton.Number.enabled = false;
				_charSelectButton.NumberBack.sprite = _lockSprite;
				_charSelectButton.NewHolder.SetActive(_characterObject.IsNew);
			}
			else
			{
				_charSelectButton.Number.enabled = true;
				_charSelectButton.gameObject.SetActive(status);
				_charSelectButton.NewHolder.SetActive(false);
			}
		}
		
		private void OnClick(Unit obj)
		{
			_characterMenuController.SelectCharacter(_characterObject);
		}

		public void Dispose()
		{
			_disposable.Dispose();	
		}

		public void SetLevel(int level, CharacterUpgradeUIData data)
		{
			if (!_charSelectButton.NumberBack.isActiveAndEnabled)
				_charSelectButton.NumberBack.gameObject.SetActive(true);
			
			_charSelectButton.Number.text = level.ToString();
			_charSelectButton.Number.color = data.TextColor;
			_charSelectButton.NumberBack.sprite = data.Sprite;
		}

		public void HideLevel()
		{
			_charSelectButton.NumberBack.gameObject.SetActive(false);
		}

		public void Deselect()
		{
			_charSelectButton.DeselectOutline();
		}
		
		public void Select()
		{
			_charSelectButton.SelectOutline();
		}
	}
}