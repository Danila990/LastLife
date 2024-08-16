using System.Linq;
using Core.Inventory.Items.Weapon;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class RaycastInteractionMiddleware : MonoInteractionMiddleware
	{
		public Collider[] validColliders;
		public override bool IsValidInteraction(EntityDamagable entityDamagable, IInteractorVisiter visiter, ref InteractionCallMeta meta)
		{
			if (visiter is IExplosionVisiter && Physics.Linecast(meta.Point, meta.OriginPoint, out var hitInfo))
			{
				if (!validColliders.Contains(hitInfo.collider))
					return false;
			}
			return true;
		}
	}
}