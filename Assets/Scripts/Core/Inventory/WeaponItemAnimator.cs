using Core.Boosts;
using Core.Entity.Characters;
using Core.Inventory.Items.Weapon;
using UnityEngine;

namespace Core.Inventory
{
	public class WeaponItemAnimator : SimpleItemAnimator
	{
		[SerializeField] private ReloadAnimationData _reloadAnimationData;

		public ReloadAnimationPlayer ReloadAnimationPlayer { get; set; }

		public void Initialize(
			CharacterContext context,
			ProjectileWeaponContext weaponContext, 
			AudioClip reloadSound, 
			float defaultSpeed,
			float processSpeed)
		{
			ReloadAnimationPlayer = new ReloadAnimationPlayer(context, _reloadAnimationData, this, weaponContext, reloadSound, defaultSpeed, processSpeed);
		}

		public override void OnUpdate(float deltaTime)
		{
			base.OnUpdate(deltaTime);
			ReloadAnimationPlayer?.OnTick();
		}
	}

}