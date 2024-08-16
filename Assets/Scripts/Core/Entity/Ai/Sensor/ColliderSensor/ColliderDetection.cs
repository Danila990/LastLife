using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Entity.Ai.Sensor.ColliderSensor
{
	public abstract class ColliderDetection : MonoBehaviour
	{
		[TitleGroup("Scan")]
		[MinMaxSlider(0, 1f)]
		[SerializeField] private Vector2 _scanPeriod;

		[SerializeField]
		private int _bufferCapacity = 4;
		
		[ShowInInspector]
		[TitleGroup("Debug")]
		[PropertyOrder(19)]
		public bool IsScanning { get; set; }
		private bool _shouldStopScanning;
		
		[ShowInInspector]
		[PropertyOrder(20)]
		private Collider[] _buffer;
		
		[ShowInInspector]
		[PropertyOrder(21)]
		private int _bufferSize;
		
		private readonly List<IColliderDetectionHandler> _handlers = new List<IColliderDetectionHandler>();
		
		private void Awake()
		{
			_buffer = new Collider[_bufferCapacity];
		}

		public void AddListener(IColliderDetectionHandler handler)
		{
			_handlers.Add(handler);
		}

		public void RemoveListener(IColliderDetectionHandler handler)
		{
			_handlers.Remove(handler);
		}

		public void StopScan()
		{
			_shouldStopScanning = true;
		}
		
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
				
				Array.Clear(_buffer, 0, _buffer.Length);
				_bufferSize = Detect(_buffer);
				OnBufferUpdate(_bufferSize, _buffer);
			}
			_shouldStopScanning = false;
			IsScanning = false;
		}

		protected virtual void OnBufferUpdate(int bufferSize, Collider[] buffer)
		{
			foreach (var handler in _handlers)
			{
				handler.OnBufferUpdate(buffer, in bufferSize);
			}
		}
		
		protected abstract int Detect(Collider[] buffer);
	}

}
