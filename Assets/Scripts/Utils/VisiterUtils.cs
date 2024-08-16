using System;
using BurstLinq;
using Core.Entity.InteractionLogic;
using UnityEngine;
using UnityEngine.Pool;

namespace Utils
{
	public static class VisiterUtils
	{
		private static readonly RaycastSort _raycastSort = new();

		public static InteractionResultMeta RayVisit(RaycastHit hit, IInteractorVisiter visiter)
		{
			if (!hit.collider.TryGetComponent(out IInteractableProvider interaction)) return StaticInteractionResultMeta.Default;
			var meta = new InteractionCallMeta { Point = hit.point,Normal = hit.normal, Distance = hit.distance, Collider = hit.collider};
			var res = interaction.Visit(visiter,ref meta);
			if (res.Interacted) return res;
			return StaticInteractionResultMeta.Default;
		}
		
		public static InteractionResultMeta CollisionVisit(Collision collision, IInteractorVisiter visiter)
		{
			if (!collision.collider.TryGetComponent(out IInteractableProvider interaction)) return StaticInteractionResultMeta.Default;

			var contact = collision.GetContact(0);
			
			var meta = new InteractionCallMeta
			{
				Point = contact.point,
				Normal = contact.normal,
				Distance = (contact.thisCollider.transform.position - contact.otherCollider.transform.position).magnitude
			};
			var res = interaction.Visit(visiter,ref meta);
			if (res.Interacted) return res;
			return StaticInteractionResultMeta.Default;
		}
		
		public static InteractionResultMeta TriggerVisit(Collider collider, IInteractorVisiter visiter)
		{
			if (!collider.TryGetComponent(out IInteractableProvider interaction)) return StaticInteractionResultMeta.Default;

			var meta = new InteractionCallMeta
			{
				Point = collider.transform.position,
				Normal = Vector3.up,
				Distance = 0f
			};
			var res = interaction.Visit(visiter,ref meta);
			if (res.Interacted) return res;
			return StaticInteractionResultMeta.Default;
		}
		
		public static InteractionResultMeta SphereCastVisit(uint selfUid, Ray ray, float radius, ref RaycastHit[] pool, float rayDist, IInteractorVisiter visiter)
		{
			var cashedIds = ListPool<uint>.Get();

			cashedIds.Add(selfUid);
            
			var hitCount = Physics.SphereCastNonAlloc(ray, radius, pool, rayDist, LayerMasks.InteractionMask);
			if (hitCount <= 0) return StaticInteractionResultMeta.Default;
			Array.Sort(pool,0,hitCount,_raycastSort);
			for (var i = 0; i < hitCount; i++)
			{
				if (!pool[i].collider.TryGetComponent(out IInteractableProvider interaction)) continue;
				if (interaction.Uid != 0 && BurstLinqExtensions.Contains(cashedIds, interaction.Uid))
					continue;
				var meta = new InteractionCallMeta { Point = pool[i].point, Normal = pool[i].normal, Distance = pool[i].distance};
				var res = interaction.Visit(visiter, ref meta);
				if (res.Interacted)
				{
					cashedIds.Add(interaction.Uid);
					if (res.HitBlock)
					{
						ListPool<uint>.Release(cashedIds);
						return res;
					}
				}
			}
			ListPool<uint>.Release(cashedIds);
			return StaticInteractionResultMeta.Default;
		}
	}
}