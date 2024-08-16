using System;
using Core.Entity.Head;
using Ui.Sandbox.Pointer;
using UniRx;
using VContainer.Unity;

namespace Core.Services
{
	public interface IBossProvider
	{
		public HeadContext CurrentContext { get; }
	}
	
	public class BossProvider : IBossProvider, IInitializable, IDisposable
	{
		private readonly IBossSpawnService _bossSpawnService;
		private readonly PointerController _pointerController;

		private HeadContext _currentContext;
		private IDisposable _disposable;
		
		public HeadContext CurrentContext => _currentContext;
		
		public BossProvider(IBossSpawnService bossSpawnService, PointerController pointerController)
		{
			_bossSpawnService = bossSpawnService;
			_pointerController = pointerController;
		}

		public void Initialize()
			=> _disposable = _bossSpawnService.CurrentBoss.Subscribe(OnBossSpawn);

		public void Dispose()
			=> _disposable?.Dispose();

		private void OnBossSpawn(HeadContext context)
		{
			_currentContext = context;
			if(context)
				_pointerController.Add(context);
		}
	}
}
