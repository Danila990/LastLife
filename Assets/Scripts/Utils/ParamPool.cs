using Db.VFXDataDto.Impl;
using UnityEngine;
using uPools;
using Object = UnityEngine.Object;

namespace Utils
{
	public class ParamPool<T> : ObjectPoolBase<T> 
		where T : Component
	{
		private readonly T _object;
		private readonly Transform _holder;

		public ParamPool(T obj, Transform holder)
		{
			_object = obj;
			_holder = holder;
		}

		protected override T CreateInstance()
		{
			var obj = Object.Instantiate(_object, _holder);
			obj.name = _object.name;
			return obj;
		}

		protected override void OnReturn(T instance)
		{
			instance.gameObject.SetActive(false);			
		}

		protected override void OnRent(T instance)
		{
			instance.gameObject.SetActive(true);
		}
	}

	public class ParamPool<T, TY> : ObjectPoolBase<T>
		where T : Component
	{
		public readonly TY Data;
		private readonly Transform _holder;
		private readonly T _object;

		public ParamPool(T obj, TY data, Transform holder, int defaultCapacity = 10)
		{
			_object = obj;
			Data = data;
			_holder = holder;
		}
		public int CountActive { get; set; }
		
		protected override T CreateInstance()
		{
			CountActive++;
			return Object.Instantiate(_object, _holder);
		}

		protected override void OnRent(T instance)
		{
			CountActive++;
		}
		
		protected override void OnReturn(T instance)
		{
			CountActive--;
		}
	}
	
	public class ParticleParamPool<TY> : ParamPool<VFXContext, TY>
	{
		private readonly Vector3 _transformLocalScale;
		public ParticleParamPool(VFXContext obj, TY data, Transform holder, int defaultCapacity = 10) : base(obj, data, holder, defaultCapacity)
		{
			_transformLocalScale = obj.transform.localScale;
		}

		public VFXContext Get()
		{ 
			var polled = Rent();
			polled.ToReturn = this;
			return polled;
		}

		public IPooled<VFXContext> GetAsDisposable()
		{
			var polled = Rent();
			polled.ToReturn = this;
			return polled;
		}

		protected override void OnReturn(VFXContext instance)
		{
			base.OnReturn(instance);
			instance.ParticleSystem.transform.localScale = _transformLocalScale;
		}
	}
}