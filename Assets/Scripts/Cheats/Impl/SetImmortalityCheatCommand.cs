using System;
using Core.Entity.Characters;
using Core.InputSystem;
using UniRx;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Cheats.Impl
{
	public class SetImmortalityCheatCommand : ICheatCommandProvider, IDisposable
	{
		private IDisposable _sub;
		public string ButtonText => "god mode";
		public bool IsToggle => true;
		
		public void Execute(bool state)
		{
			var scope = LifetimeScope.Find<LifetimeScope>(SceneManager.GetActiveScene());
			var service = scope.Container.Resolve<IPlayerSpawnService>();
			if (service.PlayerCharacterAdapter)
			{
				_sub?.Dispose();
				if (state)
				{
					service.PlayerCharacterAdapter.CurrentContext.SetImmortal(true);
					_sub = service.PlayerCharacterAdapter.ContextChanged.Subscribe(OnContextChanged);
				}
				else
				{
					service.PlayerCharacterAdapter.CurrentContext.SetImmortal(false);
				}
			}
		}
		
		private void OnContextChanged(CharacterContext context)
		{
			if (context)
			{
				context.SetImmortal(true);
			}
		}
		
		public void Dispose()
		{
			_sub?.Dispose();
		}
	}

}