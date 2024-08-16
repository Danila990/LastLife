using Core.Services;
using GameStateMachine.States.Impl.Project;
using UnityEngine.SceneManagement;

namespace Cheats.Impl
{
	public class UnlockAllCheatCommand : ICheatCommandProvider
	{
		private readonly IItemStorage _storage;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly ProjectStateMachine _projectStateMachine;

		public string ButtonText => "unlock \n all";
		public bool IsToggle => true;

		public UnlockAllCheatCommand(IItemStorage storage, IItemUnlockService itemUnlockService, ProjectStateMachine projectStateMachine)
		{
			_storage = storage;
			_itemUnlockService = itemUnlockService;
			_projectStateMachine = projectStateMachine;
		}
		
		public void Execute(bool status)
		{
			foreach (var item in _storage.All.Values)
			{
				if(status)
					_itemUnlockService.UnlockItem(item);
				else
					_itemUnlockService.LockItem(item);
			}
			
			_projectStateMachine.ChangeStateAsync<ProjectLoadSceneState, string>(SceneManager.GetActiveScene().name);
		}
	}
}