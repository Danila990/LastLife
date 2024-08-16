using Core.Entity.Ai.AiItem.Data;
using Core.Inventory.Items;
using Core.Inventory.Items.Weapon;
using Core.Inventory.Items.Weapon.Adapters;

namespace Core.Entity.Ai.AiItem
{
	public class ThrowableAiItemContext : AiItemContextDecorator
	{
		private readonly ThrowableWeaponContext _projectileWeaponContext;
		private float _elapsedTime;
		private bool _used;
		private ThrowableSpiderAdapter _throwableSpiderAdapter;

		public ThrowableAiItemContext(ItemContext context, EntityContext owner, AiItemContextedData aiItemContextedData) : base(context, owner, aiItemContextedData)
		{
			_projectileWeaponContext = (ThrowableWeaponContext)context;
		}

		protected override void OnUse(IAiTarget aiTarget)
		{
			_elapsedTime = 0;
			_used = false;
			_throwableSpiderAdapter = _projectileWeaponContext.WeaponAdapter as ThrowableSpiderAdapter;
		}

		public override void Tick(ref float deltaTime)
		{
			if (!AiTarget.IsActive)
				return;
			_throwableSpiderAdapter?.SetTargetPosition(AiTarget.PredictedLookAtPoint);
			
			if (_used)
				return;
			
			_elapsedTime += deltaTime;
			if (_elapsedTime >= 0.25f)
			{
				_projectileWeaponContext.Use(true);
				_used = true;
			}
		}

		protected override void OnEnd(bool sucsess)
		{
			_projectileWeaponContext.Use(false);
		}
	}
}