using System.Collections.Generic;
using System.Linq;
using Analytic;
using Common;
using Core.Carry;
using Core.Entity;
using Core.Entity.Repository;
using Core.Quests.Tips;
using Core.ResourcesSystem;
using Core.Timer;
using Installer;
using LitMotion;
using SharedUtils;
using UniRx;
using UnityEngine;
using VContainer;

namespace Market.OilRig
{
	public class RefinerFactoryContext : EntityContext, IInjectableTag
	{
		[SerializeField] private ResourceType _resourceType;
		[SerializeField] private float _duration;
		[SerializeField] private ProducerObject _producer;
		[SerializeField] private AudioClip _onStartRefine;
		[SerializeField] private RefinerObject[] _refinerObjects;
		[SerializeField] private MotionAnimator _motionAnimator;
		[SerializeField] private AudioSource _audioPlayer;

		[Inject] private readonly ITimerProvider _timerProvider;
		[Inject] private readonly IRefineProvider _refineProvider;
		[Inject] private readonly IRefinerBoostService _refinerBoostService;
		[Inject] private readonly IObjectResolver _resolver;
		[Inject] private readonly IEntityRepository _entityRepository;
		[Inject] private readonly ICarryInventoryService _carryInventoryService;
		[Inject] private readonly IAnalyticService _analyticService;
		//[Inject] private readonly IQuestTipService _questTipService;

		private ReactiveCommand<RefinerFactoryContext> _onEnd;
		private CompositeDisposable _disposable;
		private MotionHandle _handle;

		public ResourceType ResourceType => _resourceType;
		public IReadOnlyList<RefinerObject> RefinerObjects => _refinerObjects;
		public IReactiveCommand<RefinerFactoryContext> OnEndRefine => _onEnd;
		public ProducerObject ProducerObject => _producer;
		
		public float Duration
		{
			get
			{
				return _duration;
			}
			set
			{
				_duration = value;
			}
		}

		private void Start()
		{
			_disposable = new CompositeDisposable();
			_onEnd = new ReactiveCommand<RefinerFactoryContext>();
			_refineProvider.AddEntity(this);
			_resolver.Inject(_producer);
			//_questTipService.AddTip(QuestTip);
				
			foreach (var refiner in _refinerObjects)
			{
				_resolver.Inject(refiner);
				refiner.Init(_entityRepository);
				refiner.OnTriggered.Subscribe(OnTriggered).AddTo(_disposable);
				refiner.ProcessState.Subscribe(OnStart).AddTo(_disposable);
				refiner.OnEndEvent += OnEnd;
			}
		}

		private void OnDisable()
		{
			_disposable?.Dispose();
			_onEnd?.Dispose();
			_refineProvider?.RemoveEntity(this);
		}

		public void OnPickUp()
		{
			if(!_producer._conveyor._endPoints.Any(x => x.IsEmpty && !x.IsBookedForProducer))
				return;
			
			foreach (var refiner in _refinerObjects)
				refiner.OnPickUp();
		}

		public void OnDrop()
		{
			foreach (var refiner in _refinerObjects)
				refiner.OnDrop();
		}

		private void OnStart(bool isStarted)
		{
			if(!isStarted)
				return;
			
			_producer._conveyor.BookForProducer();
		}
		
		private void OnTriggered((CarriedContext, RefinerObject) pair)
		{
			if(!_producer._conveyor._endPoints.Any(x => x.IsEmpty && !x.IsBookedForProducer))
				return;
			
			var fuelBarrel = pair.Item1 as FuelBarrelContext;

			if (fuelBarrel == null)
				return;

			_carryInventoryService.DetachCurrent();
			LaunchRefiner(pair.Item2, fuelBarrel);
			_analyticService.SendEvent($"OilRig:Minigame:PlaceBarrel");
		}
		

		private void LaunchRefiner(RefinerObject refiner, FuelBarrelContext fuelBarrel)
		{
			var timer = GetTimer(refiner);

			LaunchRefiner(refiner, fuelBarrel, timer);
		}
		
		private ITimer GetTimer(RefinerObject refiner)
		{

#if UNITY_EDITOR
			var duration = 120f;
#else
			var duration = _duration;
#endif
			var timer = _timerProvider.AddOrGetTimer(refiner.Id, duration.ToSec());
			return timer;
		}

		public void LaunchRefiner(RefinerObject refiner, FuelBarrelContext fuelBarrel, ITimer timer)
		{
			refiner.StartRefine(fuelBarrel, timer);
			if (_onStartRefine)
			{
				_audioPlayer.clip = _onStartRefine;
				_audioPlayer.Play();
				LMotion.Create(0f, 1f, 1f).Bind(x =>
				{
					_audioPlayer.volume = x;
				}).AddTo(this);
			}
			if (_motionAnimator)
			{
				_motionAnimator.EnableAnim();
			}
		}
		
		private void OnEnd(RefinerObject refiner)
		{
			_producer.Produce();
			_timerProvider.RemoveTimer(refiner.Id);

			_onEnd.Execute(this);
			if (!_refinerObjects.Any(o => o.InProcess))
			{
				LMotion.Create(1f, 0f, 1f).Bind(x =>
				{
					_audioPlayer.volume = x;
				}).AddTo(this);
			}
			
			if (_motionAnimator)
			{
				_motionAnimator.DisableAnim();
			}
			_analyticService.SendEvent($"OilRig:Minigame:ProduceFuel");
		}
	}
}
