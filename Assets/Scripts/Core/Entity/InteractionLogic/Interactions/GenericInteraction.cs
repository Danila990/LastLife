using Core.Entity.Characters;
using Sirenix.OdinInspector;
using Ui.Sandbox.WorldSpaceUI;
using UniRx;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{

	public class GenericInteraction : ItemSupplyInteraction
	{
		protected ReactiveCommand<CharacterContext> OnUsed;
		[Inject] public readonly IObjectResolver ObjectResolver;
		public bool DontShow;
		public bool UseCustomText;
		[ShowIf("UseCustomText")]
		public string CustomText;
		public IReactiveCommand<CharacterContext> Used => OnUsed;

		public void SetDontShow(bool status)
		{
			DontShow = status;
		}
		
		protected override void OnPlayerEnter()
		{
			if (DontShow)
			{
				DisableUI();
				return;
			}
			
			base.OnPlayerEnter();
			if (UseCustomText)
			{
				var ui = (WorldSpaceSupplyBox) CurrentUI;
				ui.Count.text = CustomText;
			}
		}

		protected override void UpdateStatus(bool status)
		{
			if (DontShow)
			{
				base.UpdateStatus(false);
				return;
			}
			
			base.UpdateStatus(status);
		}

		private void OnEnable()
		{
			OnUsed = new ReactiveCommand<CharacterContext>();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			OnUsed?.Dispose();
		}

		
		protected override bool AdditionalCondition()
		{
			var player = PlayerSpawnService.PlayerCharacterAdapter.CurrentContext;

			if (!player)
				return false;

			return !player.CarryInventory.HasContext;
		}

		protected override void Select(CharacterContext characterContext)
		{
			OnUsed.Execute(characterContext);
		}
	}
}