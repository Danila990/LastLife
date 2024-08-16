using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.InteractionLogic.Interactions;
using Db.ObjectData.Impl;
using UniRx;
using UnityEngine;

namespace Core.Boosts.Entity
{
	public class BoostPickUp : MonoBehaviour
	{
		[SerializeField] private GenericInteraction _interaction;
		[SerializeField] private BoostItemObjectDataSo _boostSo;
		[SerializeField] private EntityContext _referenceEntityContext;
		public int Quantity = 1;
		
		private void Start()
		{
			_interaction.Used.Subscribe(OnUsed).AddTo(gameObject);
		}

		private void OnUsed(CharacterContext context)
		{
			if (context.Adapter is PlayerCharacterAdapter player)
			{
				player.BoostsInventory.Add(_boostSo.Model.BoostArgs, Quantity);
			}
			_referenceEntityContext.OnDestroyed(_referenceEntityContext.EntityRepository);
			Destroy(gameObject);
		}
	}
}
