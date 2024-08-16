using System.Linq;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items.Weapon;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Core.Entity.Ai.AiActions
{
	[Name("ReloadEmptyClip")]
	public class AiReloadEmptyClipAction : ActionTask<AiCharacterAdapter>
	{
		private ProjectileWeaponContext _weapon;
		
		protected override void OnExecute()
		{
			_weapon ??= (ProjectileWeaponContext)agent.CurrentContext.Inventory.InventoryItems.FirstOrDefault(context => context.ItemContext is ProjectileWeaponContext).ItemContext;
			if (!_weapon.IsClipEmpty)
			{
				EndAction(true);
				return;
			}
			
			_weapon.StartReload();
		}


		protected override void OnUpdate()
		{
			if(_weapon == null)
				return;
			
			if(!_weapon.ShouldReload)
				EndAction(true);
		}
		
		protected override void OnStop()
		{
		}
	}
}
