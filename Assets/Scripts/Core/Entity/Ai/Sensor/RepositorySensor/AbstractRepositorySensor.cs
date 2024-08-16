using System.Collections.Generic;
using Core.Entity.Repository;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using VContainer;
using Random = UnityEngine.Random;

namespace Core.Entity.Ai.Sensor.RepositorySensor
{
	public abstract class AbstractRepositorySensor<T> : MonoBehaviour where T : EntityContext
	{
		[TitleGroup("Scan")]
		[MinMaxSlider(0, 1f)]
		[SerializeField] private Vector2 _scanPeriod;
		[Inject] private readonly IEntityRepository _entityRepository;
		private bool _shouldStopScanning;
		public bool IsScanning { get; set; }
		public ICollection<T> ObservedCollection { get; set; }

		private void Start()
		{
			Init();
		}

		public void Init()
		{
			ObservedCollection = _entityRepository.GetGenericEntities<T>();
			OnInit();
		}
		protected abstract void OnInit();

		public async UniTaskVoid Scanning()
		{
			if (IsScanning)
				return;
			
			IsScanning = true;
			while (!destroyCancellationToken.IsCancellationRequested && !_shouldStopScanning)
			{
				await UniTask.Delay(
					Random.Range(_scanPeriod.x, _scanPeriod.y).ToSec(), 
					delayTiming:PlayerLoopTiming.FixedUpdate,
					cancellationToken: destroyCancellationToken);

				OnBufferUpdate(ObservedCollection);
			}
			_shouldStopScanning = false;
			IsScanning = false;
		}

		public abstract void OnBufferUpdate(ICollection<T> observedCollection);
	}
}