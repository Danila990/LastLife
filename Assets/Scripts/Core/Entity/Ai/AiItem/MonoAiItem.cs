using Core.Entity.Ai.AiItem.Data;
using Core.Factory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.Ai.AiItem
{
	public abstract class MonoAiItem : MonoBehaviour, IAiItem, IAiItemData
	{
		[field:SerializeField, TabGroup("MonoAiItem")] public string ItemId { get; set; }
		[field:SerializeField, TabGroup("IAiItemData")] public float AttackRange { get; set; }
		[field:SerializeField, TabGroup("IAiItemData")] public bool SelfEnd { get; set; }
		[field:SerializeField, TabGroup("IAiItemData")] public float Cooldown { get; set; }
		[field:SerializeField, TabGroup("IAiItemData")] public float ConstPriority { get; set; }
		[field:SerializeField, TabGroup("IAiItemData")] public float UseItemDuration { get; set; }
		[field:SerializeField, TabGroup("IAiItemData")] public float Damage { get; set; }
		[field:SerializeField, TabGroup("IAiItemData")] public bool UseRig { get; set; }
		[field:SerializeField, TabGroup("IAiItemData")] public string RigName { get; set; }
		
		[field:SerializeField, TabGroup("IAiItem")] public uint ItemUid { get; set; }
		[field:SerializeField, TabGroup("IAiItem")] public bool InUse { get; set; }
		[field:SerializeField, TabGroup("IAiItem")] public float UseRange { get; set; }
		[field:SerializeField, TabGroup("IAiItem")] public float UseActionDuration { get; set; }
		
		private float _usedInTime;
		private bool _disabled;
		public IAiTarget AiTarget { get; set; }
		public EntityContext Owner { get; set; }

		public IAiItemData AiItemData 
			=> this;
		
		public virtual void Created(EntityContext owner)
		{
			Owner = owner;
			SetUid();
		}
		public void Use(IAiTarget aiTarget)
		{
			InUse = true;
			AiTarget = aiTarget;
			OnUse(aiTarget);
		}
		
		public void EndUse(bool success)
		{
			_usedInTime = Time.time;
			InUse = false;
			OnEnd(success);
		}
		
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
		
		public virtual float GetPriority(IAiTarget aiTarget)
		{
			if (_disabled)
				return -99999;
			return AiItemData.ConstPriority - GetDistancePriority(aiTarget) - GetCurrentCooldown() + Random.value;
		}
		
		protected virtual void SetUid() 
			=> ItemUid = ObjectFactory.GetNextUid();
		
		public float GetDistanceToTarget(IAiTarget target) 
			=> Vector3.Distance(target.MovePoint, Owner.MainTransform.position);
		
		public void Disable() 
			=> _disabled = true;
		
		public bool IsValidItem(IAiTarget aiTarget) 
			=> true; //Validator (By Range)

		protected abstract void OnUse(IAiTarget aiTarget);
		protected abstract void OnEnd(bool sucsess);
		public abstract void Tick(ref float deltaTime);
		public abstract void OnAnimEvent(object args);
		public abstract void Dispose();
	}
}