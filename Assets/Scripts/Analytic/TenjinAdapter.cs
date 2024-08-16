#if RELEASE_BRANCH && tenjin
using System;
using UnityEngine;
using VContainer.Unity;

namespace Analytic
{
	public class TenjinAdapter : IAnalyticAdapter, IPostStartable, IDisposable
	{
		private readonly BaseTenjin _instance;
		
		public TenjinAdapter()
		{
			_instance = Tenjin.getInstance("GBHRYRGXKSHYR9AF36DBNVY8PYYFC3XU");
			_instance.SetAppStoreType(AppStoreType.googleplay);
			_instance.Connect();
		}
		
		public void Send(string args)
		{
			//_instance.SendEvent(args);
		}

		private void ApplicationOnfocusChanged(bool pauseStatus)
		{
			if (!pauseStatus) {
				_instance.Connect();
			}
		}
		
		public void Dispose()
		{
			Application.focusChanged -= ApplicationOnfocusChanged;
		}
		
		public void PostStart()
		{
			Application.focusChanged += ApplicationOnfocusChanged;
		}
	}
}
#endif