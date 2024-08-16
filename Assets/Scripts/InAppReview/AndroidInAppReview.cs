using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using System.Threading;
#endif

#if RELEASE_BRANCH
using Google.Play.Review;
#endif

namespace InAppReview
{
#if UNITY_ANDROID
	public class AndroidInAppReview : IInAppReview
	{
#if RELEASE_BRANCH
		private ReviewManager _reviewManager;
		private PlayReviewInfo _playReviewInfo;
#endif

		public UniTask RequestInAppReview(CancellationToken token)
		{
			return InitReview(token, false);
		}
		
		public async UniTask InitReview(CancellationToken token, bool force = false)
		{
#if RELEASE_BRANCH
			_reviewManager ??= new ReviewManager();

			var requestFlowOperation = _reviewManager.RequestReviewFlow();
			await requestFlowOperation.ToUniTask(cancellationToken:token);
			if (requestFlowOperation.Error != ReviewErrorCode.NoError)
			{
				if (force) 
					DirectlyOpen();
				
				return;
			}

			_playReviewInfo = requestFlowOperation.GetResult();
			
			var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
			await launchFlowOperation.ToUniTask(cancellationToken:token);

			_playReviewInfo = null; // Reset the object

			if (launchFlowOperation.Error != ReviewErrorCode.NoError)
			{
				DirectlyOpen();
			}
#else
			await UniTask.Yield(token);
			DirectlyOpen();
#endif
		}
		

		private void DirectlyOpen()
		{
#if UNITY_EDITOR
			Debug.Log("IAP REVIEW TO " + $"https://play.google.com/store/apps/details?id={Application.identifier}");
#else
			Application.OpenURL($"market://details?id={Application.identifier}");
#endif
		}
		
	}
	#endif
}
