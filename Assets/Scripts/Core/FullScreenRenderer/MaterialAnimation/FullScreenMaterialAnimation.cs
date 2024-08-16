using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.FullScreenRenderer.MaterialAnimation
{
	public abstract class FullScreenMaterialAnimation : ScriptableObject
	{
		private CancellationToken _token;
		protected CancellationToken Token => _token;
		protected Material Material;
		
		protected bool HasToken => _token != CancellationToken.None;
		

		public void SetCancellationToken(CancellationToken token)
		{
			Release();
			_token = token;
		}
		
		public void SetMaterialInstance(Material matInstance)
		{
			if (Material)
			{
				Release();
				Destroy(Material);
			}

			Material = matInstance;
		}

		public void FadeIn(Action callback = null)
		{
			if(!Validate())
				return;
			
			Release();
			FadeInInternalAwait(callback).Forget();
		}
		
		public void FadeOut(Action callback = null)
		{
			if(!Validate())
				return;
			
			Release();
			FadeOutInternalAwait(callback).Forget();
		}
		
		public void Fade(Action callback = null)
		{
			if(!Validate())
				return;
			
			Release();
			FadeInternalAwait(callback).Forget();
		}
		
		protected abstract UniTaskVoid FadeInInternalAwait(Action callback = null);
		protected abstract UniTaskVoid FadeOutInternalAwait(Action callback = null);
		protected abstract UniTaskVoid FadeInternalAwait(Action callback = null);

		public abstract void Release();

		private bool Validate()
		{
			if (!HasToken)
			{
				Debug.LogError($"{this} doesn't have a cancellation token");
				return false;
			}
			
			if (Material == null)
			{
				Debug.LogError($"{this} doesn't have a material");
				return false;
			}

			return true;
		}
	}

}
