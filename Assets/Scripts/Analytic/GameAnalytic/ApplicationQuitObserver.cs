#if RELEASE_BRANCH
using System;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Analytic.GameAnalytic
{
    public interface IApplicationQuitObserver
    {
        public event Action OnAppQuit;
    }
    
    public class ApplicationQuitObserver : IStartable, IApplicationQuitObserver
    {
        private readonly IApplicationQuitSettings _settings;
        public event Action OnAppQuit;

        public ApplicationQuitObserver(
            IApplicationQuitSettings settings
        )
        {
            _settings = settings;
        }

        public void Start()
        {
            var obj = Object.Instantiate(_settings.Bridge);
            obj.transform.SetParent(null);
            Object.DontDestroyOnLoad(obj);
            obj.OnAppQuit += AppQuit;
        }

        private void AppQuit()
        {
            OnAppQuit?.Invoke();
        }

    }
}
#endif