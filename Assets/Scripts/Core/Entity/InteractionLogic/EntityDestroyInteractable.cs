using Core.Entity.Characters;
using Core.Entity.Repository;
using UnityEngine;

namespace Core.Entity.InteractionLogic
{
	public class EntityDestroyInteractable : IInteractableContexted
	{
		public uint Uid => _entityContext.Uid;
		private EntityContext _entityContext; 
		
		
		public void Destroy(IEntityRepository entityRepository)
		{
			_entityContext.OnDestroyed(entityRepository);
			Object.Destroy(_entityContext.gameObject);
		}
		
		public InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
		{
			return visiter.Accept(this, ref meta);
		}
		
		public void SetCharContext(CharacterContext context)
		{
			_entityContext = context;
		}
		
		public void SetEntityContext(EntityContext context)
		{
			_entityContext = context;
		}
	}
}