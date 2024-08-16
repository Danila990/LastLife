using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Db.VFXDataDto.Impl;
using GameSettings;
using SharedUtils;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Unity;
using VFX;

namespace Core.Factory.VFXFactory.Impl
{
	public class VFXFactory : IVFXFactory, IInitializable, IDisposable
	{
		private readonly IVFXData _vfxData;
		private readonly IObjectResolver _resolver;
		private readonly Dictionary<string, ParticleParamPool<ParticleData>> _pools = new Dictionary<string, ParticleParamPool<ParticleData>>();
		private readonly CancellationDisposable _cancellationDisposable = new CancellationDisposable();
		private readonly Dictionary<int, (VFXContext particleSystem, IDisposable disposable)> _waitingDestroy = new Dictionary<int, (VFXContext particleSystem, IDisposable disposable)>();
		private Transform _vfxHolder;

		public VFXFactory(IVFXData vfxData, IObjectResolver resolver)
		{
			_vfxData = vfxData;
			_resolver = resolver;
		}
		
		
		public void Initialize()
		{
			_vfxHolder = new GameObject("VFX Pool").transform;
			
			Stack<VFXContext> temp = new Stack<VFXContext>(10);
			foreach (var particleData in _vfxData.ParticleData)
			{
			
				var pool = new ParticleParamPool<ParticleData>(particleData.Particle, particleData, _vfxHolder, particleData.MaxCount);
				_pools.Add(particleData.Name, pool);
				temp.Clear();
				
				var preloadCount = pool.Data.PreloadCount;
				pool.Prewarm(preloadCount);
			}

		}

		public bool CreateAndForget(string name, Vector3 pos)
		{
			return CreateAndForget(name, pos, Vector3.up);
		}
		
		public bool CreateAndForget(string name, Vector3 pos, Vector3 normal)
		{
			if (!TryGetParticle(name, out var vfxContext))
				return false;
			var rot = Quaternion.LookRotation(normal);
			vfxContext.SetPosAndRotation(pos, rot);
			vfxContext.Play();
			AutoRelease(name,vfxContext);
			return true;
		}

		public bool TryGetParticle(string name, out IPooled<VFXContext> pooled, out VFXContext vfxContext)
		{
			if (GameSetting.ViolenceStatus){
				vfxContext = default;
				pooled = default;
				return false;
			}
			var pool = _pools[name];
			if (pool.CountActive >= pool.Data.MaxCount)
			{
				vfxContext = default;
				pooled = default;
				return false;
			}

			vfxContext = pool.Get();
			pooled = vfxContext;
			return true;
		}

		public bool TryGetParticle(string name, out VFXContext vfxContext)
		{
			if (GameSetting.ViolenceStatus){
				vfxContext = default;
				return false;
			}
			var pool = _pools[name];
			if (pool.CountActive >= pool.Data.MaxCount)
			{
				vfxContext = null;
				return false;
			}

			vfxContext = pool.Get();
			
			if(vfxContext.Components != null)
				foreach (var component in vfxContext.Components)
					_resolver.Inject(component);
			
			return true;
		}

		public void AutoRelease(string name, VFXContext vfxContext)
		{
			WaitUntilComplete(name, vfxContext).Forget();
		}
		
		public void ReleaseOnDestroy(string name, VFXContext vfxContext, Component target)
		{
			var disposable = target.OnDestroyAsObservable().SubscribeWithState2(vfxContext, name, OnDestroy);
			_waitingDestroy.Add(vfxContext.GetInstanceID(), (vfxContext, disposable));
		}
		
		public void ReleaseOnDestroyAndForget(string name, VFXContext vfxContext, Component target)
		{
			var disposable = target.OnDestroyAsObservable().SubscribeWithState2(vfxContext, name, OnDestroy);
			_waitingDestroy.Add(vfxContext.GetInstanceID(), (vfxContext, disposable));
			WaitUntilCompleteReleaseOnDestroy(name, vfxContext).Forget();
		}

		public void ReleaseFromOnDestroy(VFXContext vfxContext, string name)
		{
			var id = vfxContext.GetInstanceID();
			if (id == 0)
				return;
			var valueTuple = _waitingDestroy[vfxContext.GetInstanceID()];
			valueTuple.disposable?.Dispose();
			_waitingDestroy.Remove(id);
		}
		
		private void OnDestroy(Unit arg1, VFXContext vfxContext, string name)
		{
			var id = vfxContext.GetInstanceID();
			if (id == 0)
				return;
			var onDestroy = _waitingDestroy[id];
			onDestroy.disposable?.Dispose();
			_waitingDestroy.Remove(id);
			Release(name, vfxContext);
		}

		private async UniTaskVoid WaitUntilComplete(string name, VFXContext vfxContext)
		{
			await UniTask.Delay(vfxContext.ParticleSystem.main.duration.ToSec(), cancellationToken: _cancellationDisposable.Token);
			Release(name, vfxContext);
		}
		
		private async UniTaskVoid WaitUntilCompleteReleaseOnDestroy(string name, VFXContext vfxContext)
		{
			await UniTask.Delay(vfxContext.ParticleSystem.main.duration.ToSec(), cancellationToken: _cancellationDisposable.Token);
			ReleaseFromOnDestroy(vfxContext, name);
			Release(name, vfxContext);
		}
		
		public void Release(string name, VFXContext vfxContext)
		{
			if (!_vfxHolder || !_vfxHolder.gameObject.activeInHierarchy)
				return;
			vfxContext.Release();
			vfxContext.transform.SetParent(_vfxHolder);
			_pools[name].Return(vfxContext);
		}
		
		public void Dispose()
		{
			_cancellationDisposable?.Dispose();
			foreach (var value in _waitingDestroy.Values)
			{
				value.disposable?.Dispose();
			}
		}
	}
}