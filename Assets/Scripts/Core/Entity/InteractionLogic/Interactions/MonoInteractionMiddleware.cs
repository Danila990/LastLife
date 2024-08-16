using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public abstract class MonoInteractionMiddleware : MonoBehaviour
	{
		public abstract bool IsValidInteraction(EntityDamagable entityDamagable, IInteractorVisiter visiter, ref InteractionCallMeta meta);
	}

}