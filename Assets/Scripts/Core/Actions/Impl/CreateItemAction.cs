using Ui.Sandbox.SpawnMenu;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(CreateItemAction), fileName = nameof(CreateItemAction))]
	public class CreateItemAction : ItemEntityAction
	{
		[Inject] private readonly ISpawnItemService _spawnItemService;
		
		public override void OnDeselect()
		{
			
		}
		public override void OnInput(bool state)
		{
			
		}
		
		public override void OnInputUp()
		{
			
		}
		
		public override void OnInputDown()
		{
			_spawnItemService.Spawn();
		}
	}
}