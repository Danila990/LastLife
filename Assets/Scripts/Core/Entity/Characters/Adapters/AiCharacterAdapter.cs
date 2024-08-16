using System;
using System.Linq;
using Core.Boosts.Impl;
using Core.Effects;
using Core.Entity.Ai;
using Core.Entity.Ai.Sensor.RepositorySensor;
using Core.Entity.EntityAnimation;
using Core.Entity.Repository;
using Core.Factory.VFXFactory;
using Core.HealthSystem;
using Core.Inventory.Items.Weapon;
using Core.Services;
using Core.Zones;
using Cysharp.Threading.Tasks;
using Db.VFXDataDto.Impl;
using GameSettings;
using LitMotion;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using SharedUtils;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Characters.Adapters
{
	public class AiCharacterAdapter : BaseCharacterAdapter
	{
		[SerializeField] private NavMeshAgent _navMeshAgent;
		[SerializeField] private CharacterEffector _effector;
		[SerializeField] private Blackboard _aiBlackboard;
		[SerializeField] private BehaviourTreeOwner _behaviourTreeOwner;
		[SerializeField] private CharacterAiAdapterRepSensor _repSensor;
		[SerializeField] private string _reloadFxName;
		[SerializeField] private CharacterAnimatorAdapter _characterAnimatorAdapter;
		private CharacterData _data;
		private VFXContext _vfxContext;
		private bool _binded;
		private bool _isRagdollEnabled;
		
		private IAiTarget _aiTarget;
		private IEntityRepository _entityRepository;
		private IVFXFactory _vfxFactory;
		private ISpawnPointProvider _spawnPointProvider;
		private IObjectResolver _resolver;
		
		private IDisposable _deathTimer;
		private IDisposable _inBoundsTracking;
		private bool _isDestroyed;
		private ISettingsService _settingsService;
		private IMapZoneService _mapZoneService;

		public NavMeshAgent NavMeshAgent => _navMeshAgent;
		public override BaseEntityEffector Effector => _effector;
		public float DissolveDuration => 1f;
		public override AnimatorAdapter AnimatorAdapter => _characterAnimatorAdapter;
		public override CharacterAnimatorAdapter CharacterAnimatorAdapter => _characterAnimatorAdapter;
		public override IBoostProvider BoostProvider => null;
		public override StatsProvider StatsProvider => null;
		public IObjectResolver Resolver => _resolver;
		public IMapZoneService MapZoneService => _mapZoneService;
		public IAiTarget CurrentTarget => _aiBlackboard.GetVariable<IAiTarget>("AiTarget").value;
		
		[Inject] 
		private void Construct(
			IObjectResolver resolver, 
			IEntityRepository entityRepository,
			IVFXFactory vfxFactory,
			ISpawnPointProvider spawnPointProvider,
			ISettingsService settingsService,
			IMapZoneService zoneService)
		{
			_settingsService = settingsService;
			_vfxFactory = vfxFactory;
			_entityRepository = entityRepository;
			_spawnPointProvider = spawnPointProvider;
			resolver.Inject(_repSensor);
			resolver.Inject(_effector);
			_resolver = resolver;
			_mapZoneService = zoneService;
		}


		public override void SetEntityContext(CharacterContext characterContext)
		{
			base.SetEntityContext(characterContext);
			_data = CurrentContext.CharacterData;
			_characterAnimatorAdapter.OnContextSet(characterContext.CharacterAnimator, characterContext);
			_aiBlackboard.SetVariableValue("binded", true);
			_aiBlackboard.SetVariableValue("speed", _data.MoveSpeed);
			_aiBlackboard.SetVariableValue("RigProvider", characterContext.RigProvider);
			_navMeshAgent.transform.position = characterContext.CharacterController.position;
			characterContext.CharacterController.SetParent(transform);
			characterContext.PuppetMaster.targetRoot = transform;
			_navMeshAgent.height = _data.ColliderHeight;
			_navMeshAgent.radius = _data.AgentRadius;
			characterContext.PuppetMaster.solverIterationCount = AdapterData.SolverIteration;
			characterContext.CharacterAnimator.Animator.SetBool(AHash.GroundedParameterHash, true);
			characterContext.CharacterAnimator.Animator.SetBool(AHash.WalkParameterHash, true);
			characterContext.RigProvider.Rigs["aim"].DisableRig();
			characterContext.Health.OnDamage.Subscribe(OnGetDamage).AddTo(this);
			characterContext.Health.OnDeath.Subscribe(OnDeath).AddTo(this);
			characterContext.LegsAnimator.HipsHeightStepBlend = .4f;
			_binded = true;
			Rotate().Forget();
			OnContextSetup();
			var weapon = (ProjectileWeaponContext)
				characterContext.Inventory.InventoryItems
					.FirstOrDefault(context => context.ItemContext is ProjectileWeaponContext).ItemContext;
			weapon.Reloading.Subscribe(OnReloading).AddTo(this);
			
			_effector.SetContext(characterContext);

			StartTrackInBounds();
		}
		
		private void StartTrackInBounds()
		{
			if(_inBoundsTracking != null)
				return;
			
			AtMap();
			
			_inBoundsTracking = Observable
				.Interval(5f.ToSec())
				.Subscribe(_ =>
				{
					if (AtMap())
						StopDeathTimer();
					else
						StartDeathTimer();
				}).AddTo(this);
		}
		
		private bool IsGrounded()
		{
			Debug.DrawRay(transform.position + Vector3.up, -Vector3.up * 2f, Color.red, 4F);
			return Physics.Raycast(transform.position + Vector3.up, -Vector3.up, 2f, LayerMasks.Environment);
		}
		
		private bool AtMap()
		{
			var isGrounded = IsGrounded();
			var inBoundsMap = _spawnPointProvider.InBoundsMap(transform.position) && isGrounded;
			
			if (!isGrounded && !_isRagdollEnabled && CurrentContext)
			{
				CurrentContext.RagDollManager.EnableRagDollWithFrameDelay().Forget();
				_isRagdollEnabled = true;
			}
			else if (isGrounded && _isRagdollEnabled && CurrentContext)
			{
				CurrentContext.RagDollManager.DisableRagDollWithFrameDelay().Forget();
				_isRagdollEnabled = false;
			}
			
			if (!_navMeshAgent.isOnNavMesh && inBoundsMap)
				RefreshAgent();
			
			_aiBlackboard.SetVariableValue("AtMap", inBoundsMap && _navMeshAgent.isOnNavMesh);

			return inBoundsMap && _navMeshAgent.isOnNavMesh;
		}

		private void RefreshAgent()
		{
			_navMeshAgent.enabled = false;
			_navMeshAgent.enabled = true;
		}
		
		public void Pause()
		{
			_aiBlackboard.SetVariableValue("binded", false);
			NavMeshAgent.isStopped = true;
		}
		
		private void StartDeathTimer()
		{
			if(_deathTimer != null)
				return;
			_deathTimer = Observable.Timer(5f.ToSec())
				.TakeUntilDisable(this)
				.Subscribe(_ =>
				{
					if (!AtMap())
						DeathTask().Forget();
				});
		}

		private void StopDeathTimer()
		{
			if (_deathTimer == null)
				return;
			
			_deathTimer?.Dispose();
			_deathTimer = null;
		}

		private void OnDeath(DiedArgs diedArgs)
		{
			_inBoundsTracking?.Dispose();
			
			if(_navMeshAgent)
				_navMeshAgent.enabled = false;

			if (TryGetComponent(out Collider collider))
				collider.enabled = false;
			
			if (_vfxContext)
			{
				_vfxContext.ParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
				_vfxFactory.Release(_reloadFxName, _vfxContext);
				_vfxContext = null;
			}
			
			
			var removeAfterDeath = _settingsService.QualityPreset.SelectedPreset.Value.GetValue<bool>(SettingsConsts.REMOVE_AFTER_DEATH, GameSetting.ParameterType.Bool);
			if (removeAfterDeath)
			{
				DeathTimer().Forget();
			}	
		}


		private async UniTaskVoid Rotate()
		{
			await UniTask.NextFrame(destroyCancellationToken);
			CurrentContext.CharacterController.rotation = transform.rotation;
		}

		private void OnReloading(bool isReloading)
		{
			if (isReloading)
			{
				if (_vfxFactory.TryGetParticle(_reloadFxName, out _vfxContext))
				{
					var vfxTransform = _vfxContext.transform;
					vfxTransform.SetParent(transform);
					vfxTransform.position = transform.position + Vector3.up * (_data.ColliderHeight * 1.1f);
					_vfxContext.ParticleSystem.Play();
				}
				
				return;
			}
			if (_vfxContext)
			{
				_vfxContext.ParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
				_vfxFactory.Release(_reloadFxName, _vfxContext);
				_vfxContext = null;
			}
		}
		
		protected override void BeforeDestroy()
		{
			_binded = false;
			if(_behaviourTreeOwner)
				_behaviourTreeOwner.StopBehaviour();
			
			if(_aiBlackboard)
				_aiBlackboard.SetVariableValue("binded", false);
			
			if (CurrentContext)
			{
				Destroy(CurrentContext.CharacterController.gameObject);
				Destroy(CurrentContext.gameObject);
			}
			Destroy(gameObject);
		}

		public async UniTaskVoid DeathTimer()
		{
			var timer = _settingsService.QualityPreset.SelectedPreset.Value.GetValue<int>(SettingsConsts.REMOVE_AFTER_DEATH_TIMER, GameSetting.ParameterType.Int);
			await UniTask.Delay(timer, cancellationToken: destroyCancellationToken);
			DeathTask().Forget();
		}
		
		public async UniTaskVoid DeathTask()
		{
			if (_isDestroyed)
				return;
			
			_isDestroyed = true;
			await LMotion
				.Create(0f, 1f, DissolveDuration)
				.WithCancelOnError()
				.Bind(SetDissolve)
				.ToUniTask(destroyCancellationToken);
			await UniTask.Delay(0.1f.ToSec(), cancellationToken: destroyCancellationToken);
			
			if(CurrentContext != null)
				CurrentContext.OnDestroyed(_entityRepository);
			
			Destroy(gameObject);
		}

		private void SetDissolve(float value)
		{
			if (!CurrentContext)
				return;
			foreach (var render in CurrentContext.BodyRenders)
			{
				foreach (var material in render.materials)
				{
					material.SetFloat(ShHash.DissolveSlider, value);
				}
			}
		}

		public void SetBinded(bool status)
		{
			_aiBlackboard.SetVariableValue("binded", status);
		}
		
		public override void OnGetEffect(ref EffectArgs args)
		{
			Effector.DoEffect(args);
		}
		
		
		private void OnGetDamage(DamageArgs args)
		{
			if(args.DamageSource is null) 
				return;
			
			if (args.DamageSource.Uid == CurrentContext.Uid) 
				return;
			
			if (!args.DamageSource.TryGetAiTarget(out _aiTarget))
				return;

			if (_aiBlackboard)
			{
				_aiBlackboard.SetVariableValue("AiTarget", _aiTarget);
				_aiBlackboard.SetVariableValue("IsMeleeDamage", args.DamageType == DamageType.Melee);
			}
			
		}

		public void Update()
		{
			if (!_binded || CurrentContext.Health.IsDeath)
				return;
			
			CurrentContext.CharacterAnimator.Animator.SetBool(AHash.MovingParameterHash, _navMeshAgent.desiredVelocity.magnitude > 0.01f);
		}
	}
}