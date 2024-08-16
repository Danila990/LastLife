using Core.Effects;
using Core.Entity.Ai;
using Core.Entity.Characters.Adapters;
using Core.HealthSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity
{
	public abstract class LifeEntity : EntityContext, ICameraTargetEntity
	{
		public MeshProvider MeshProvider;
		
#if UNITY_EDITOR
		[ProgressBar(0, "MaxHealth", 0, 1,0, Height = 25), HideLabel, ShowInInspector, PropertyOrder(-1)] 
		public float HealthProgress
		{
			get => Health.CurrentHealth;
			set => Health.SetCurrentHealth(value);
		}
		public float MaxHealth => Health.MaxHealth;
#endif
		public abstract ILifeEntityHealth Health { get; }
		
		[TitleGroup("Ai")]
		public Faction Faction;
		[field:SerializeField] public Transform CameraTargetRoot { get; set; }
		public bool TargetIsActive => !IsDestroyed && !Health.IsDeath;
		public float AdditionalCameraDistance { get; protected set; } = 0;
	}

	public interface IControllableEntity 
	{
		IEntityAdapter Adapter { get; }
	}

	public interface IExperienceEntity
	{
		float ExperienceCount { get; }
		Vector3 GetExpPosition();
	}
	
}