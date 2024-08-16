using Core.Carry;
using Core.Entity.Characters;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class CarriedItemPickUpInteraction : ItemSupplyInteraction
	{
		[SerializeField] private CarriedContext _currentContext;
	
		public override void Use(EntityContext user)
		{
			if (user is CharacterContext characterContext)
			{
				Select(characterContext);
				PlaySound();
			}
		}

		protected override bool AdditionalCondition()
		{
			var player = PlayerSpawnService.PlayerCharacterAdapter.CurrentContext;

			if (!player)
				return false;

			return !player.CarryInventory.HasContext;
		}
		

		protected override void UpdateStatus(bool status)
		{
			if(_currentContext.IsAttached)
			{
				base.UpdateStatus(false);
				return;
			}
			
			base.UpdateStatus(status);
		}

		protected override void Select(CharacterContext characterContext)
		{
			characterContext.CarryInventory.Take(_currentContext);
		}
	}
}
