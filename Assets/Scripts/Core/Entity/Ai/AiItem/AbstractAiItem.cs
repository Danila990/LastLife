using Core.Entity.Ai.AiItem.Data;
using Core.Factory;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Entity.Ai.AiItem
{
	public abstract class AbstractAiItem : IAiItem
	{
		private float _usedInTime;
		public EntityContext Owner { get; set; }
		public uint ItemUid { get; protected set; }
		public bool InUse { get; private set; }
		public IAiTarget AiTarget { get; private set; }
		private readonly IAiItemData _aiItemData;
		private bool _disabled;

		public float UseRange => _aiItemData.AttackRange;
		public virtual float UseActionDuration => _aiItemData.UseItemDuration;
		
		public IAiItemData AiItemData => _aiItemData;

		public AbstractAiItem(EntityContext owner, IAiItemData aiItemData)
		{
			_aiItemData = aiItemData;
			Owner = owner;
		}

		public virtual void Created() => SetUid();
		protected virtual void SetUid() => ItemUid = ObjectFactory.GetNextUid();
		protected abstract void OnUse(IAiTarget aiTarget);
		protected abstract void OnEnd(bool sucsess);

		public void Use(IAiTarget aiTarget)
		{
			InUse = true;
			AiTarget = aiTarget;
			OnUse(aiTarget);
		}

		public virtual void Tick(ref float deltaTime) { }

		public void EndUse(bool success)
		{
			_usedInTime = Time.time;
			InUse = false;
			OnEnd(success);
		}
		
		public void Disable()
		{
			_disabled = true;
		}

		public virtual float GetPriority(IAiTarget aiTarget)
		{
			if (_disabled)
			{
				return -99999;
			}
			return AiItemData.ConstPriority - GetDistancePriority(aiTarget) - GetCurrentCooldown() + Random.value;
		}

		public bool IsValidItem(IAiTarget aiTarget)
		{
			return true; //Validator (By Range)
		}
		
		public virtual void OnAnimEvent(object args) { }

		protected float GetCurrentCooldown()
		{
			var elapsed = Time.time - _usedInTime;
			return Mathf.Max(AiItemData.Cooldown - elapsed, 0);
		}
		
		protected virtual float GetDistancePriority(IAiTarget aiTarget)
		{
			var rangePriority = UseRange - GetDistanceToTarget(aiTarget);
			return Mathf.Abs(rangePriority);
		}
		
		public float GetDistanceToTarget(IAiTarget target) => Vector3.Distance(target.MovePoint, Owner.MainTransform.position);

		public virtual void Dispose()
		{
			
		}
	}
}