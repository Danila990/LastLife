using System;
using uPools;

namespace Db.VFXDataDto.Impl
{
	public interface IPooled<T> : IDisposable where T : class
	{
		IObjectPool<T> ToReturn { get; set; } 
		void IDisposable.Dispose()
		{
			ToReturn.Return(this as T);
		}
	}
}