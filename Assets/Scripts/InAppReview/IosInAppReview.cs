using System.Threading;
using Cysharp.Threading.Tasks;

namespace InAppReview
{
	public class IosInAppReview : IInAppReview
	{
		public async UniTask RequestInAppReview(CancellationToken token)
		{
#if UNITY_IOS
			UnityEngine.iOS.Device.RequestStoreReview();
#endif
			await UniTask.NextFrame(token);
		}
	}
}