using Core.Entity.Ai.AiItem.Data;
using Core.Inventory.Items;
using Core.Inventory.Items.Weapon;

namespace Core.Entity.Ai.AiItem
{
	public abstract class AiItemContextDecorator : AbstractAiItem
	{
		protected readonly ItemContext Context;
		
		public AiItemContextDecorator(ItemContext context, EntityContext owner, AiItemContextedData aiItemContextedData) : base(owner, aiItemContextedData)
		{
			Context = context;
		}

		protected override void SetUid()
		{
			ItemUid = Context.Uid;
		}
	}

	public class ShootingAiItemContext : AiItemContextDecorator
	{
		private readonly ProjectileWeaponContext _projectileWeaponContext;
		private float _elapsedTime;
		private bool _used;
		
		public ShootingAiItemContext(
			ProjectileWeaponContext context,
			EntityContext owner, 
			ProjectileWeaponAiItemContextedData projectileWeaponAiItemContextedData) 
			: base(context, owner, projectileWeaponAiItemContextedData)
		{
			_projectileWeaponContext = context;
		}
		
		protected override void OnUse(IAiTarget aiTarget)
		{
			_elapsedTime = 0;
			_used = false;
		}

		public override void Tick(ref float deltaTime)
		{
			if (_used)
				return;
			_elapsedTime += deltaTime;
			if (_elapsedTime >= 0.25f)
			{
				_projectileWeaponContext.SetShootStatus(true);
				_used = true;
			}
		}

		protected override void OnEnd(bool sucsess)
		{
			_projectileWeaponContext.SetShootStatus(false);
		}
	}
}