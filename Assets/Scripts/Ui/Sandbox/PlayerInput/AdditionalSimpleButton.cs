using Core.Entity.Characters;

namespace Ui.Sandbox.PlayerInput
{
	public class AdditionalSimpleButton : AdditionalInputActionButton
	{
		private CharacterContext _characterContext;
		
		public override void OnContextChanged(CharacterContext characterContext)
		{
			_characterContext = characterContext;
			base.OnContextChanged(characterContext);
			if (characterContext)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}

		public override void Hide()
		{
			if (_characterContext)
				return;	
			
			base.Hide();
		}
	}
}