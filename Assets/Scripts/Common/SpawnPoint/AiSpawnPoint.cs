using Core.Entity.Characters.Adapters;
using Core.Factory;
using Db.ObjectData;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Common.SpawnPoint
{
	public class AiSpawnPoint : SpawnPoint
	{
		[field:ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Character)")]
		[field:SerializeField] public string AiAdapterId { get; set; }
		[field:SerializeField] public AiType AiType { get; set; }

		public CharacterObjectData ObjectData { get; protected set; }
		
		private void Awake()
		{
			ObjectData = new CharacterObjectData()
			{
				AiAdapterId = AiAdapterId,
				AiType = AiType
			};
		}

		public override void Create(IAdapterStrategyFactory strategyFactory)
		{
			if (!gameObject.activeInHierarchy)
				return;
			strategyFactory.CreateAiAdapter(ObjectData, transform.position, transform.rotation);
		}
		public IEntityAdapter CreateAiAdapter(IAdapterStrategyFactory strategyFactory)
		{
			if (!gameObject.activeInHierarchy)
				return null;
			return strategyFactory.CreateAiAdapter(ObjectData, transform.position, transform.rotation);
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawSphere(transform.position, 0.5f);
		}
	}
}