using System.Linq;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items.Weapon;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using UnityEngine;

namespace Core.Entity.Ai.AiActions
{
    public class AiShootAction : ActionTask<AiCharacterAdapter>
	{
		public BBParameter<IAiTarget> Target;

		private ProjectileWeaponContext _weapon;
		
		protected override void OnExecute()
		{
			_weapon ??= (ProjectileWeaponContext)agent.CurrentContext.Inventory.InventoryItems.FirstOrDefault(context => context.ItemContext is ProjectileWeaponContext).ItemContext;
			if (!_weapon)
			{
				EndAction(false);
				return;
			}
		}

		protected override void OnUpdate()
		{
			if (!Target.value.IsActive)
			{
				EndAction(false);
				return;
			}
			if(!_weapon) return;
			var delta = (Target.value.LookAtPoint - agent.transform.position).normalized;
			delta.y = 0;
			var ang = Vector3.Angle(delta, agent.transform.forward);
			_weapon.SetShootStatus(ang < 1f);
		}

		protected override void OnStop()
		{
			if (_weapon)
			{
				_weapon.SetShootStatus(false);
			}
		}
	}

}