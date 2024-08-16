using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils;
using UnityEngine;
using UnityEngine.Pool;

namespace Common.SignalRecieverCommon
{
	public class MaterialAnimationTask : MonoBehaviour
	{
		[SerializeField] private Renderer[] _renderers;
		[SerializeField] private string _hashName;
		
		[SerializeField] private float _from;
		[SerializeField] private float _to;
		[SerializeField] private float _duration;
		
		private int _hash;
		private CompositeMotionHandle _compositeMotionHandle;

		private void OnDisable()
		{
			_compositeMotionHandle?.Cancel();
		}
		
		public void Animation()
		{
			Task().Forget();
		}

		public async UniTaskVoid Task()
		{
			await AsyncAnimation(_from, _to, _duration, destroyCancellationToken);
			await UniTask.Delay(0.25f.ToSec(), cancellationToken:destroyCancellationToken);
			await AsyncAnimation(_to, _from, _duration, destroyCancellationToken);
		}
		
		public async UniTask AsyncAnimation(float from, float to, float duration, CancellationToken token = default)
		{
			var taskPool = ListPool<UniTask>.Get();
			foreach (var ren in _renderers)
			{
				if (ren.sharedMaterials.Length == 1)
				{
					taskPool.Add(LMotion
						.Create(from, to, duration)
						.BindToMaterialFloat(ren.material, _hashName)
						.ToUniTask(cancellationToken: token));
				}
				else
				{
					foreach (var mat in ren.materials)
					{
						taskPool.Add(LMotion
							.Create(from, to, duration)
							.BindToMaterialFloat(mat, _hashName)
							.ToUniTask(cancellationToken: token));
					}
				}
			}
			
			try
			{
				await UniTask.WhenAll(taskPool);
			}
			finally
			{
				ListPool<UniTask>.Release(taskPool);
			}
		}
	}
}