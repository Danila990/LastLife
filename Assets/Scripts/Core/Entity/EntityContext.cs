using System.Collections.Generic;
using System.Linq;
using Core.Entity.Ai;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.Factory.VFXFactory;
using Core.HealthSystem;
using Core.Quests.Tips;
using Core.Quests.Tips.Impl;
using Core.Quests.Tips.Impl.Interfaces;
using Core.Services;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Entity
{
	public abstract class EntityContext : MonoBehaviour, IEntity, IInteractable
	{
		[SerializeField] protected List<InteractionData> Interactions;
		[SerializeField] protected MonoInteractProvider[] Providers;
		[SerializeField] private QuestTipContext _tipContext;
		
		[Inject] private readonly IVFXFactory _vfxFactory;
		[Inject] private readonly IAudioService _audioService;
		[Inject] public readonly IEntityRepository EntityRepository;

		private ReactiveCommand _onDestroyCommand;
		private string _factoryId;

		public virtual Transform MainTransform => transform;
		public virtual Transform LookAtTransform => transform;
		public IVFXFactory VFXFactory => _vfxFactory;
		public IAudioService AudioService => _audioService;
		public IReadOnlyList<InteractionData> EntityInteractions => Interactions;
		public IReactiveCommand<Unit> OnDestroyCommand => _onDestroyCommand;
		public IQuestTip QuestTip => _tipContext;
		
		public string SourceId => _factoryId;
		
		public uint Uid { get; set; } = 0;
		public bool IsDestroyed { get; set; } = false;
		
		public void Created(IObjectResolver resolver,string factoryId)
		{
			_factoryId = factoryId;
			_onDestroyCommand = new ReactiveCommand();
			Interactions.Add(new InteractionData { InteractionType = InteractionType.InterfaceInteraction, InterfaceInteraction = new EntityDestroyInteractable()});
			foreach (var provider in Providers)
			{
				provider.Inject(resolver);
			}
			OnCreated(resolver);
		}

		protected virtual void OnCreated(IObjectResolver resolver){}
		
		public virtual InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
		{
			foreach (var interaction in Interactions)
			{
				var inter = interaction.GetInteraction();
				SetContext(inter);
				var result = inter.Visit(visiter, ref meta);
				if (result.Interacted)
				{
					return result;
				}
			}
			return StaticInteractionResultMeta.Default;
		}

		public virtual bool TryGetAiTarget(out IAiTarget result)
		{
			result = null;
			return false;
		}
		
		protected virtual void SetContext(IInteractableContexted inter)
		{
			inter.SetEntityContext(this);
		}

		public virtual void DoDamage(ref DamageArgs args, DamageType type)
		{
			
		}
		
		public virtual void DoEffect(ref EffectArgs args)
		{
			
		}
		
		public virtual void OnDestroyed(IEntityRepository entityRepository)
		{
			entityRepository?.RemoveEntity(this);
			if (!IsDestroyed)
			{
				_onDestroyCommand?.Execute();
				_onDestroyCommand?.Dispose();
			}
			
			IsDestroyed = true;
		}
		
#if UNITY_EDITOR
		public IEnumerable<MeshFilter> GetMeshFilters()
		{
			 return GetComponentsInChildren<MeshFilter>();
		}
		
		public IEnumerable<SkinnedMeshRenderer> GetSkinnedMeshRenderers()
		{
			return GetComponentsInChildren<SkinnedMeshRenderer>().Where(x => x.gameObject.activeSelf);
		}
#endif
	}
}