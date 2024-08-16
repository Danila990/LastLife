using Core.Entity.Ai.AiItem.Data;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Inventory.Items.Weapon;
using UnityEngine;

namespace Core.Entity.Head
{

	public class BackTankDamagable : EntityDamagable, ISpecialExplosionHandler, ISnapListener
	{
		[SerializeField] private AiItemContextedData _itemData;
		[SerializeField] private Renderer _notBroken;
		[SerializeField] private Renderer _broken;
		[SerializeField] private Collider _collider;
		private bool _canBroken;

		public bool IsBroken { get; private set; }

		public override InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
		{
			if (_collider && meta.Collider)
			{
				if(!_collider.Equals(meta.Collider)) return StaticInteractionResultMeta.Default;
			}
			visiter.Accept(this,ref meta);
			return StaticInteractionResultMeta.Default;
		}
		
		public void Handle()
		{
			if (IsBroken || _canBroken == false)
				return;
			
			IsBroken = true;
			_notBroken.enabled = false;
			_broken.enabled = true;
			if (TargetContext is HeadContext { CurrentAdapter: AiHeadAdapter adapter} )
			{
				adapter.AiInventorySensor.DisableItem(_itemData);
			}		
		}
		
		public void OnSnap(SnapObject snapObject)
		{
			if (snapObject.TryGetComponent(out RemoteExplosionEntity remoteExplosionEntity))
			{
				_canBroken = true;
			}	
		}
	}
}