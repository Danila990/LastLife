using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Ai;
using Core.Entity.Characters;
using Core.Entity.Repository;
using Core.Factory;
using Core.HealthSystem;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Entity.Head.Bird
{
	public class BirdEggEntity : LifeEntity, IExperienceEntity
	{
		[SerializeField] private CharacterHealth _health;
		
		
		[SerializeField] private GameObject _broken;
		[SerializeField] private GameObject _normal;
		[SerializeField] private AudioClip _onDeathClip;
		[SerializeField] private AudioClip _brokingClip;
		[SerializeField] private string _particleOnDeath;
		[SerializeField] private Transform _lookAtTransform;

		[SerializeField] private ParticleSystem _brokeFx;
		
		public override ILifeEntityHealth Health => _health;

		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.AllIds")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[SerializeField] private string _entityToSpawnId;
		[SerializeField] private float _timeToSpawn;
		[Inject] private readonly IObjectFactory _factory;
		[Inject] private readonly IEntityRepository _entityRepository;

		public readonly ReactiveCommand<IObservable<DiedArgs>> OnSpawnOrDestroyChild = new ReactiveCommand<IObservable<DiedArgs>>();
		private MotionHandle _handle;
		private IDisposable _timer;
		private EntityTarget _selfTarget;
		public override Transform LookAtTransform => _lookAtTransform;
		public float ExperienceCount => 10f;
		
		protected override void OnCreated(IObjectResolver resolver)
		{
			_health.Init();
			_health.SetContext(this);
			_health.OnDeath.Subscribe(OnDeath).AddTo(this);
			
			_timer = Observable
				.Timer(_timeToSpawn.ToSec())
				.Subscribe(SpawnEntity);

			_handle = LMotion.Punch
				.Create(Vector3.zero, Vector3.forward * 2, 1f)
				.WithLoops(-1)
				.BindToLocalEulerAngles(transform);
		}
		
		public Vector3 GetExpPosition() => MainTransform.position + new Vector3(0, MeshProvider.MainRenderer.localBounds.max.y);

		private void OnDeath(DiedArgs obj)
		{
			_timer?.Dispose();

			OnSpawnOrDestroyChild.Execute(null);
			VFXFactory.CreateAndForget(_particleOnDeath, transform.position, Vector3.up);
			
			if (_onDeathClip)
			{
				AudioService
					.PlayNonQueue(_onDeathClip)
					.SetPosition(transform.position)
					.SetSpatialBlend(1);
			}
			_normal.SetActive(false);
			_broken.SetActive(true);
			
			OnDestroyed(_entityRepository);
			
			Destroy(gameObject, 2f);
		}

		private void PlayBroke()
		{
			if(!_brokingClip)
				return;

			LucidAudio
				.PlaySE(_brokingClip)
				.SetPosition(_broken.transform.position)
				.SetVolume(1f)
				.SetSpatialBlend(1f);
		}
		
		private void SpawnEntity(long obj)
		{
			var miniBird = _factory.CreateObject(_entityToSpawnId, transform.position);
			_normal.SetActive(false);
			_broken.SetActive(true);
			PlayBroke();

			_brokeFx.Play();
			OnSpawnOrDestroyChild.Execute(((LifeEntity)miniBird).Health.OnDeath);
			OnDestroyed(_entityRepository);
			Destroy(gameObject, 3f);
		}
		
		public override void DoDamage(ref DamageArgs args, DamageType type)
		{
			_health.DoDamage(ref args);
		}

		private void OnDisable()
		{
			_handle.IsActiveCancel();
			_timer?.Dispose();
		}

		public override bool TryGetAiTarget(out IAiTarget result)
		{
			if (_health.IsDeath)
			{
				result = null;
				return false;
			}
			_selfTarget ??= new EntityTarget(this);
			result = _selfTarget;
			return true;
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			_timer?.Dispose();
			_handle.IsActiveCancel();
			entityRepository.RemoveGenericEntity((LifeEntity)this);
		}
		
	}
}