using System.Collections.Generic;
using Core.Entity;
using Core.Entity.Ai.Merchant;
using Core.Factory;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Common.SpawnPoint
{
	public class MerchantSpawnPoint : ObjectSpawnPoint
	{
		[SerializeField, HorizontalGroup("pointsOfInterest")] private string _pointsOfInterestKey; 
		[SerializeField, HorizontalGroup("pointsOfInterest")] private List<GameObject> _pointsOfInterest;

		public readonly ReactiveCommand<MerchantEntityContext> MerchantCreated = new ReactiveCommand<MerchantEntityContext>();
		
		public override EntityContext CreateObject(IObjectFactory objectFactory, bool destroy)
		{
			var merchant = base.CreateObject(objectFactory, destroy);
			OnCreated(merchant);
			return merchant;
		}

		public override void Create(IAdapterStrategyFactory strategyFactory)
		{
			var merchant = (MerchantEntityContext)strategyFactory.CreateObject(_objectData, transform.position, transform.rotation);
			OnCreated(merchant);
			Destroy(gameObject,Random.Range(0.01f, 1f));
		}

		private void OnCreated(object entity)
		{
			var merchant = (MerchantEntityContext)entity;
			MerchantCreated.Execute(merchant);
			merchant.Blackboard.SetVariableValue(_pointsOfInterestKey, _pointsOfInterest);
			MerchantCreated?.Dispose();
		}
	}
}