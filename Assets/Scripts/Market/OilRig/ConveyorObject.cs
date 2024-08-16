using System;
using System.Collections.Generic;
using System.Linq;
using AnnulusGames.LucidTools.Audio;
using Core.Carry;
using Core.Entity;
using Core.Quests.Messages;
using DG.Tweening;
using Installer;
using LitMotion;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer;
using Ease = DG.Tweening.Ease;
using LoopType = LitMotion.LoopType;

namespace Market.OilRig
{
	public class ConveyorObject : MonoBehaviour, IInjectableTag
	{
		[SerializeField] private string _id;
		[SerializeField, BoxGroup("Params")] private Transform[] _path;
		[SerializeField, BoxGroup("Params")] public EndPoint[] _endPoints;
		[SerializeField, BoxGroup("Params")] private Renderer _renderer;
		[SerializeField, BoxGroup("Params")] private float _moveDuration = 5f;
		[SerializeField, BoxGroup("Params")] private float _velocity;
		[SerializeField, BoxGroup("Params")] private AudioClip _clip;
		[SerializeField, BoxGroup("Params")] private Transform _soundPoint;

		[Inject] private readonly IConveyorProvider _conveyorProvider;
		[Inject] private readonly IQuestMessageSender _questMessageSender;

		
		private AudioPlayer _player;
		private MotionHandle _movement;
		private CompositeDisposable _disposable;
		
		private readonly Dictionary<CarriedContext, (Tween Tween, IDisposable Sub)> _tweens = new();
		private readonly BoolReactiveProperty _canPlace = new BoolReactiveProperty(true);
		private readonly ReactiveCommand<ConveyorObject> _onModified = new ReactiveCommand<ConveyorObject>();
		private readonly ReactiveCommand<Unit> _onComplete = new();

		public IReactiveProperty<bool> CanPlace => _canPlace;
		public IReactiveCommand<ConveyorObject> OnModified => _onModified;
		public ReactiveCommand<Unit> OnComplete => _onComplete;

		public string Id => _id;
		
		private void Start()
		{
			_disposable = new CompositeDisposable();
			_conveyorProvider.Register(this);
			foreach (var point in _endPoints)
				point.OnReleased.Subscribe(OnReleased).AddTo(_disposable);
		}

		public void Place(CarriedContext context)
		{
			if(_tweens.ContainsKey(context) || !CanPlace.Value)
				return;
			
			_questMessageSender.SendProduceMessage(context.SourceId, 1);
			var endPoint = _endPoints.First(x => x.IsEmpty && !x.IsBooked);
			Book();
			var points = _path.Select(x => x.position).ToArray();
			var tween = context.transform
				.DOPath(points, _moveDuration, PathType.CatmullRom, gizmoColor: Color.green)
				.SetEase(Ease.Linear)
				.SetLookAt(0.01f)
				.OnStart(() =>
				{
					context.IsAttached = true;
					context.Rigidbody.isKinematic = true;
				})
				.OnComplete(() =>
				{
					PlaceIntoPoint(context, endPoint);
				});

			_tweens[context] = (tween, context.OnStateChanged.Subscribe(Complete));
			LaunchContainer();
			RecalcCanPlace();
		}

		private void OnReleased(Unit _)
		{
			_onModified?.Execute(this);
			RecalcCanPlace();
		}
		
		private void PlaySound()
		{
			_player = LucidAudio
				.PlaySE(_clip)
				.SetSpatialBlend(1f)
				.SetVolume(0.3f)
				.SetPosition(_soundPoint.position)
				.SetLoop();
		}

		private void StopSound()
		{
			if (_player != null && _player.state != AudioPlayer.State.Stop)
				_player.Stop();
		}
		
		private void OnDisable()
		{
			_canPlace?.Dispose();
			_onModified?.Dispose();
			StopSound();
			foreach (var point in _endPoints)
				point?.Dispose();
		}

		public void Book()
		{
			_endPoints.First(x => x.IsEmpty && !x.IsBooked ).IsBooked = true;
		}
		
		public void BookForProducer()
		{
			var endPoint = _endPoints.FirstOrDefault(x => x.IsEmpty && !x.IsBooked && !x.IsBookedForProducer);
			if (endPoint != null)
				endPoint.IsBookedForProducer = true;
		}
		
		public void RecalcCanPlace()
		{
			_canPlace.Value = _endPoints.Any(x => x.IsEmpty && !x.IsBooked);
		}

		private void LaunchContainer()
		{
			if(_movement.IsActive())
				return;
			
			PlaySound();
			_movement = LMotion
				.Create(0, _velocity * _moveDuration, _moveDuration)
				.WithLoops(-1, LoopType.Incremental)
				.BindWithState(_renderer.materials.Last(), BindToMaterial);
		}

		private void BindToMaterial(float val, Material mat)
		{
			mat.SetFloat(ShHash.Shift, val);
		}

		public void Complete(CarriedContext context)
		{
			if (_tweens.TryGetValue(context, out var pair))
			{
				if(!context.IsAttached)
					context.Rigidbody.isKinematic = false;
				
				pair.Sub?.Dispose();
				pair.Tween.Kill();
				_tweens.Remove(context);
			}

			if (_tweens.Count == 0 && _movement.IsActive())
			{
				_movement.Complete();
			}
			RecalcCanPlace();
		}

		private void PlaceIntoPoint(CarriedContext context, EndPoint endPoint)
		{
			endPoint.Place(context);
			_onModified?.Execute(this);
			
			if (_tweens.TryGetValue(context, out var pair))
			{
				pair.Sub?.Dispose();
				pair.Tween.Kill();
				_tweens.Remove(context);
			}
			
			if (_tweens.Count == 0 && _movement.IsActive())
			{
				_movement.Cancel();
				StopSound();
			}
			
			RecalcCanPlace();
			_onComplete.Execute(new Unit());
		}

		public bool CanPlaceIntoPoint(string endPointId)
		{
			return _endPoints.Any(x => x.Id == endPointId);
		}
		
		public void PlaceIntoPoint(CarriedContext context, string endPointId)
		{
			var endPoint = _endPoints.FirstOrDefault(x => x.Id == endPointId);
			if(endPoint == null)
				return;
			
			endPoint.Place(context);
			_onModified?.Execute(this);

			RecalcCanPlace();
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			foreach (var endPoint in _endPoints)
			{
				Gizmos.DrawWireSphere(endPoint.Position, 0.25f);
			}
			for (var index = 0; index < _path.Length; index++)
			{
				var point = _path[index];
				if (index + 1 < _path.Length)
				{
					Gizmos.DrawLine(point.position, _path[index + 1].position);
				}
			}
		}
#endif
		
		[Serializable]
		public class EndPoint : IDisposable
		{
			[SerializeField] private Transform Point;
			[SerializeField] private string _id;
			
			private ReactiveCommand _onReleased = new ReactiveCommand();
			private CarriedContext _currentContext;
			private IDisposable _disposable;

			public bool IsBooked;
			public bool IsBookedForProducer;
			
			public bool IsEmpty => _currentContext == null;
			public EntityContext Context => _currentContext;
			public IReactiveCommand<Unit> OnReleased => _onReleased;
			public Vector3 Position => Point.position;
			public string Id => _id;
			
			public void Place(CarriedContext context)
			{
				if(!IsEmpty)
					return;

				context.CanSave = false;
				context.IsAttached = false;
				_currentContext = context;
				_currentContext.MainTransform.position = Point.position;
				_currentContext.MainTransform.rotation = Point.rotation;
				_currentContext.ChangePhysic(true);
				_disposable = _currentContext.OnStateChanged.Subscribe(_ => OnRelease());
			}

			private void OnRelease()
			{
				_currentContext.CanSave = true;
				_currentContext = null;
				_disposable?.Dispose();
				IsBooked = false;
				IsBookedForProducer = false;
				_onReleased?.Execute();
			}

			public void Dispose()
			{
				_onReleased?.Dispose();
				_disposable?.Dispose();
			}
		}
	}
}
