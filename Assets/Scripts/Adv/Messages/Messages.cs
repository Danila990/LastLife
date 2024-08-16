using System;

namespace Adv.Messages
{
	public readonly struct MessageShowAdReady { }
	public readonly struct MessageHideAd { }
	public readonly struct MessageShowAd { }

	public readonly struct MessageShowAdWindow
	{
		public readonly Action<Action, string> Callback;
		public readonly Action OnComplete;
		public readonly string InterstitialMeta;

		public MessageShowAdWindow(Action<Action, string> showInter, Action onComplete, string interstitialMeta)
		{
			Callback = showInter;
			OnComplete = onComplete;
			InterstitialMeta = interstitialMeta;
		}
	}
	
	public readonly struct ShowShopMenu
	{
		public readonly string LastDropId;
		public readonly string ID;
		public readonly bool ForceUseMenuPanel;
		public bool CanClose { get; }


		public ShowShopMenu(string id, string lastDropId = null, bool forceUseMenuPanel = false, bool canClose = true)
		{
			LastDropId = lastDropId;
			ID = id;
			CanClose = canClose;
			ForceUseMenuPanel = forceUseMenuPanel;
		}
	}
}