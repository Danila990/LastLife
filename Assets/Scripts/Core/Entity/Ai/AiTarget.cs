using System;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head;
using Core.Entity.InteractionLogic;
using UnityEngine;

namespace Core.Entity.Ai
{
	[Serializable]
	public class EntityTarget : IAiTarget
	{
		private readonly LifeEntity _entityContext;
		public Faction Faction => _entityContext.Faction;
		public Vector3 MovePoint => _entityContext.MainTransform.position;
		public Vector3 LookAtPoint => _entityContext.LookAtTransform.position;
		public Vector3 PredictedLookAtPoint => GetPredictedPosition();
		public bool HasEntity => Entity != null;
		public float AgentRadius { get; private set; }
		public float AgentHeight { get; private set; }
		public bool IsImmortal => !_destroyed && _entityContext.Health.IsImmortal;
		public uint Uid => _entityContext.Uid;
		public bool IsActive => !_destroyed && _entityContext && !_entityContext.Health.IsDeath;
		private bool _destroyed;
		public LifeEntity Entity => _entityContext;
		
		public EntityTarget(CharacterContext characterContext)
		{
			_entityContext = characterContext;
			AgentRadius = characterContext.CharacterData.AgentRadius;
			AgentHeight = characterContext.CharacterData.ColliderHeight;
		}
		
		public EntityTarget(LifeEntity lifeEntity)
		{
			_entityContext = lifeEntity;
			AgentRadius = 5;
			AgentHeight = 5;
		}
		
		public void OnDestroy()
		{
			_destroyed = true;
		}

		private Vector3 GetPredictedPosition()
		{
			if (Entity is CharacterContext { Adapter: PlayerCharacterAdapter characterAdapter })
			{
				return characterAdapter.Rigidbody.velocity + LookAtPoint;
			}
			return LookAtPoint;
		}

		public InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
		{
			return Entity.Visit(visiter, ref meta);
		}
	}
	
	public interface IAiTarget : IInteractable
	{
		bool IsActive { get; }
		Faction Faction { get; }
		Vector3 MovePoint { get; }
		Vector3 LookAtPoint { get; }
		Vector3 PredictedLookAtPoint { get; }
		public bool HasEntity { get; }
		float AgentRadius { get; }
		float AgentHeight { get; }
		bool IsImmortal { get; }
	}
}