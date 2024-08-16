using Core.Entity.Characters;
using Core.HealthSystem;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public interface IArmorPart
	{
		public EntityDamagable Damagable { get; }
	}
	
	public class CharacterPartArmored : CharacterPartDamagable
	{
		private IArmorPart _armorPart;

		public void SetArmorRef(IArmorPart part)
		{
			_armorPart = part;
		}

		public override void DoDamageWithEffects(ref DamageArgs args, Vector3 pos, Vector3 normal, DamageType type)
		{
			if (HasArmoredPart())
			{
				_armorPart.Damagable.DoDamageWithEffects(ref args, pos, normal, type);
				return;
			}
			
			base.DoDamageWithEffects(ref args, pos, normal, type);
			
		}

		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			if (HasArmoredPart())
			{
				_armorPart.Damagable.DoDamage(ref args, type);
				return;
			}

			base.DoDamage(ref args, type);
		}
		

		public override void DoDamageBullet(ref DamageArgs args, Vector3 pos, Vector3 normal, Vector3 bulletVel)
		{
			if (HasArmoredPart())
			{
				_armorPart.Damagable.DoDamageBullet(ref args, pos, normal, bulletVel);
				return;
			}

			base.DoDamageBullet(ref args, pos, normal, bulletVel);
		}

		public override void DoDamageExplosion(ref DamageArgs args, Vector3 pos, Vector3 normal, Vector3 direction)
		{
			if (HasArmoredPart())
			{
				_armorPart.Damagable.DoDamageExplosion(ref args, pos, normal, direction);
				return;
			}

			base.DoDamageExplosion(ref args, pos, normal, direction);
		}

		private bool HasArmoredPart() => _armorPart != null && _armorPart.Damagable;
	}
}
