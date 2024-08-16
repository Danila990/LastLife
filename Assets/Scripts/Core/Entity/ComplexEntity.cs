using Core.Entity.Repository;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Entity
{
	public sealed class ComplexEntity : EntityContext
	{
		public EntityContext MainEntity;

		[SerializeField] private Component[] _injectables;
		
		
		public void ManualInit(IObjectResolver resolver,string factoryId)
		{
			foreach (var injectable in _injectables)
				resolver.Inject(injectable);
			
			if (!MainEntity || MainEntity == this)
				return;
			
			resolver.Inject(MainEntity);
			MainEntity.Created(resolver,factoryId);
		}
		
		public string FactoryId { get; set; }
		public bool CanSave { get; set; }
	}
}
