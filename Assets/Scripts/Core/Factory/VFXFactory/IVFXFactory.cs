using Db.VFXDataDto.Impl;
using UnityEngine;

namespace Core.Factory.VFXFactory
{
	public interface IVFXFactory
	{
		bool TryGetParticle(string name, out VFXContext vfxContext);
		bool CreateAndForget(string name, Vector3 pos);
		bool CreateAndForget(string name, Vector3 pos, Vector3 normal);
		bool TryGetParticle(string name, out IPooled<VFXContext> pooled, out VFXContext vfxContext);
		void Release(string name, VFXContext vfxContext);
		void AutoRelease(string name, VFXContext vfxContext);
		
		void ReleaseOnDestroy(string name, VFXContext vfxContext, Component target);
		void ReleaseFromOnDestroy(VFXContext vfxContext, string name);
		public void ReleaseOnDestroyAndForget(string name, VFXContext vfxContext, Component target);
	}
}