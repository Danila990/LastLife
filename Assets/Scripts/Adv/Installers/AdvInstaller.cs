using Adv.Providers;
using Adv.Services;
using UnityEngine;
using VContainer;
using VContainer.Extensions;

namespace Adv.Installers
{
	public class AdvInstaller : MonoInstaller
	{
		[SerializeField] private bool _advTestMode;
		
		public override void Install(IContainerBuilder builder)
		{
			
#if RELEASE_BRANCH
			if (_advTestMode)
			{
				builder.Register<MockAdvProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			}
			else
			{
				//builder.Register<CasAdvProvider>(Lifetime.Singleton).AsImplementedInterfaces();
			}
#else
			builder.Register<MockAdvProvider>(Lifetime.Singleton).AsImplementedInterfaces();
#endif
			
			builder.Register<SimpleAdvService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<RemoveAdsService>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}
}