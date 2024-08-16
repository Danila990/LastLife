using System;
using Core.Carry;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.InputSystem;
using Core.Quests.Messages;
using Core.Timer;
using Db.Map;
using LitMotion;
using LitMotion.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainer;

namespace Market.OilRig
{
	public class RefinerObject : MonoBehaviour
	{
		[SerializeField] private string _id;
		[SerializeField] private Transform _point;
		[SerializeField] private Vector3 _offset;
		[SerializeField] private RefinerUIController _uiController;
		[SerializeField] private Collider _trigger;

		[Inject] private readonly IPlayerSpawnService _spawnService;
		[Inject] private readonly IQuestMessageSender _questMessageSender;
		
		private CompositeDisposable _disposable;
		private CarriedContext _context;
		private IEntityRepository _entityRepository;
		private ReactiveCommand<(CarriedContext, RefinerObject)> _onTriggered;
		private BoolReactiveProperty _processState;
		
		public event Action<RefinerObject> OnEndEvent;
		public bool InProcess { get; private set; }
		public ITimer Timer { get; private set; }
		public string Id => _id;

		public IReactiveCommand<(CarriedContext, RefinerObject)> OnTriggered => _onTriggered;
		public IReactiveProperty<bool> ProcessState => _processState;

		public void Init(IEntityRepository entityRepository)
		{
			_entityRepository = entityRepository;
			_uiController.Init(_spawnService.PlayerCharacterAdapter);
			_onTriggered = new ReactiveCommand<(CarriedContext, RefinerObject)>();
			_processState = new BoolReactiveProperty();
		}

		public void OnPickUp()
		{
			_uiController.SetArrowStatus(true);
		}

		public void OnDrop()
		{
			_uiController.SetArrowStatus(false);
		}

		private void LateUpdate()
		{
			_uiController?.LateTick();
		}

		private void OnTriggerEnter(Collider other)
		{
			if(InProcess)
				return;

			var barrel = other.GetComponentInChildren<CarriedContext>();
			if(!barrel)
				return;

			_onTriggered?.Execute((barrel, this));
		}

		public void StartRefine(FuelBarrelContext context, ITimer timer)
		{
			if(InProcess)
				return;

			_processState.Value = true;
			_trigger.enabled = false;
			Timer = timer;
			InProcess = true;
			
			_context = context;
			_context.OnDestroyed(_entityRepository);

			Attach(context);
			_disposable = new CompositeDisposable();
			Timer.OnEnd.Subscribe(_ => OnEnd()).AddTo(_disposable);
		}

		private void OnDisable()
		{
			_disposable?.Dispose();
			_uiController.Dispose();
			_onTriggered?.Dispose();
			_processState?.Dispose();
			_trigger.enabled = false;
		}

		private void OnEnd()
		{
			_processState.Value = false;
			_disposable?.Dispose();
			_uiController.Dispose();
			Destroy(_context.gameObject);
			InProcess = false;
			_context = null;
			_trigger.enabled = true;
			
			OnEndEvent?.Invoke(this);
		}
		
		private void Attach(CarriedContext context)
		{
			_uiController.AttachTimer(Timer);
			context.OnAttach();
			_questMessageSender.SendCarryMessage(context.SourceId);
			Transform tempTransform;
			(tempTransform = context.transform).SetParent(_point);
			tempTransform.localPosition = Vector3.zero;
			tempTransform.up = _point.up;
		}
		
	}

	[Serializable]
	public class RefinerUIController
	{
		[SerializeField] private Image _arrow;
		[SerializeField] private Image _filledImage;
		[SerializeField] private GameObject _timerUi;
		[SerializeField] private Transform _container;

		private Vector3 _defaultArrowPosition;
		private MotionHandle _handle;
		private ITimer _timer;
		private CompositeDisposable _disposable;
		private PlayerCharacterAdapter _adapter;

		public void Init(PlayerCharacterAdapter adapter)
		{
			_adapter = adapter;
			_defaultArrowPosition = _arrow.transform.position;
		}

		public void LateTick()
		{
			if (!_adapter.CurrentContext)
				return;

			if(!_arrow.enabled && !_timerUi.activeInHierarchy)
				return;
				
			var direction = (_adapter.CurrentContext.MainTransform.position - _container.position);
			direction.y = 0;
			_container.rotation = Quaternion.LookRotation(direction);
		}
		
		public void SetArrowStatus(bool status)
		{
			if (_timer == null && _arrow.enabled != status)
				_arrow.enabled = status;

			if (!status)
			{
				_handle.IsActiveCancel();
				return;
			}
			
			_arrow.transform.position = _defaultArrowPosition;
			_handle.IsActiveCancel();
			_handle = LMotion
				.Create(_arrow.transform.position, _arrow.transform.position - Vector3.down * 0.3f, 1f)
				.WithLoops(-1, LoopType.Yoyo)
				.WithEase(Ease.InSine)
				.BindToPosition(_arrow.transform);
		}

		public void AttachTimer(ITimer timer)
		{
			SetArrowStatus(false);
			_timerUi.SetActive(true);
			
			_timer = timer;
			_disposable?.Dispose();
			_disposable = new CompositeDisposable();
			_timer.ElapsedTime.Subscribe(OnTimerTick).AddTo(_disposable);
		}

		private void OnTimerTick(TimeSpan timeSpan)
		{
			_filledImage.fillAmount = (float)(timeSpan.TotalSeconds / _timer.TotalTime.TotalSeconds);
		}

		public void Dispose()
		{
			_timer = null;
			_timerUi.SetActive(false);
			_handle.IsActiveCancel();
			_disposable?.Dispose();
		}
	}
}
