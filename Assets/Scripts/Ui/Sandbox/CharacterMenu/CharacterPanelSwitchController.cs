using System;
using UniRx;

namespace Ui.Sandbox.CharacterMenu
{
	public class CharacterPanelSwitchController : IDisposable
	{
		private readonly CharacterScreenData _characterScreen;
		private readonly CharacterScreenData _skillsScreen;

		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private CharacterScreenData _selectedScreen;

		public CharacterPanelSwitchController(
			CharacterScreenData characterScreen,
			CharacterScreenData skillsScreen)
		{
			_characterScreen = characterScreen;
			_skillsScreen = skillsScreen;

			_characterScreen.Widget.Button.OnClickAsObservable().SubscribeWithState(_characterScreen, OnClickCharacterScreen).AddTo(_compositeDisposable);
			_skillsScreen.Widget.Button.OnClickAsObservable().SubscribeWithState(_skillsScreen, OnClickCharacterScreen).AddTo(_compositeDisposable);
			SelectCharacterScreen();
		}
		
		private void OnClickCharacterScreen(Unit arg1, CharacterScreenData screen) => SelectScreen(screen);
		public void SelectCharacterScreen() => SelectScreen(_characterScreen);
		public void SelectSkillsScreen() => SelectScreen(_skillsScreen);

		private void SelectScreen(CharacterScreenData screen)
		{
			if (_selectedScreen.Widget)
			{
				_selectedScreen.Widget.SelectAlpha(0f);
				_selectedScreen.Screen.SetActive(false);
			}
			
			_selectedScreen = screen;
			_selectedScreen.Widget.DeselectAlpha(0f);
			_selectedScreen.Screen.SetActive(true);
		}

		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}