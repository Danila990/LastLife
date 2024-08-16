#if RELEASE_BRANCH
using Analytic.AppodeadAnalytic;
using Analytic.GameAnalytic;
using GameAnalyticsSDK;
using UnityEngine;
#endif

using VContainer;
using VContainer.Extensions;


namespace Analytic.AnalyticInstaller
{
    public class ProjectAnalyticInstaller : MonoInstaller
    {
        
#if RELEASE_BRANCH
        [SerializeField] private GameAnalytics _gameAnalytics;
#endif
        
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<AnalyticService>(Lifetime.Singleton).AsImplementedInterfaces();
            
#if UNITY_EDITOR
            builder.Register<DebugAnalyticAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
#endif
            
#if RELEASE_BRANCH
            var gameAnalytics = Instantiate(_gameAnalytics);
            DontDestroyOnLoad(gameAnalytics);
            
            builder.Register<GameAnalyticAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
#if tenjin
            builder.Register<TenjinAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
#endif
	        builder.Register<AppodealAnalyticAdapter>(Lifetime.Singleton).AsImplementedInterfaces();

#endif
        }
    }
}
