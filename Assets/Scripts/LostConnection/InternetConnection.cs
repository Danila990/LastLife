using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace LostConnection
{
	public static class InternetConnection
	{
		public static bool HasInternet { get; private set; }
		public static IReactiveProperty<bool> HasInternetObservable { get; set; }

		private static readonly string[] Uri =
		{
			"https://google.com",
			"https://www.wikipedia.org/",
			"https://www.youtube.com/",
			"https://www.twitch.tv/"
		};

		public async static UniTask<bool> CheckInternetConnection(CancellationToken cancellationToken)
		{
			foreach (var t in Uri)
			{
				var unityWebRequest = new UnityWebRequest(t);
				try
				{
					await unityWebRequest.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
				}
				catch (UnityWebRequestException e)
				{
					Debug.LogError("NO INTERNET" + e.Message);
				}
                
				if (unityWebRequest.error == null)
				{
					HasInternet = true;
					if (HasInternetObservable != null)
					{
						HasInternetObservable.Value = true;
					}
					return true;
				}
			}

			
			if (HasInternetObservable != null)
			{
				HasInternetObservable.Value = false;
			}			
			return false;
		}
	}
}