using System;
using Core.Actions;
using VContainer;
using VContainer.Unity;

namespace Core.Services
{
	public class ScriptableActionsInitializer : IInitializable, IDisposable
	{
		private readonly IScriptableActionsData _actionsData;
		private readonly IObjectResolver _resolver;
		
		public ScriptableActionsInitializer(IScriptableActionsData actionsData, IObjectResolver resolver)
		{
			_actionsData = actionsData;
			_resolver = resolver;
		}
		
		public void Initialize()
		{
			foreach (var actionsData in _actionsData.Actions)
			{
				_resolver.Inject(actionsData);
				actionsData.Initialize();
			}
		}
		
		public void Dispose()
		{
			foreach (var actionsData in _actionsData.Actions)
			{
				actionsData.Dispose();
			}
		}
	}
}