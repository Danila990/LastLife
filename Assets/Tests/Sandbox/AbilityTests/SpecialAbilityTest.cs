using System.Collections;
using ControlFreak2;
using Core.Actions.SpecialAbilities;
using Core.InputSystem;
using Core.Services;
using Cysharp.Threading.Tasks;
using Tests.Mock;
using Tests.UITest;
using UnityEngine.TestTools;
using Utils.Constants;
using VContainer;
using VContainer.Extensions;

namespace Tests.Sandbox.AbilityTests
{
	public class SpecialAbilityTest : UiTest
	{
		protected override void OverrideRegistration()
		{
			InstallerSubstitution.Substitute<IItemUnlockService, MockUnlockService>();
		}

		[UnityTest]
		public IEnumerator SpecialAbilities() => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitSandbox(CancellationToken);
			var playerSpawnService = scope.Container.Resolve<IPlayerSpawnService>();
			var abilitiesController = scope.Container.Resolve<IAbilitiesControllerService>();

			await UniTask.Delay(2000, cancellationToken: CancellationToken);
			var context = playerSpawnService.PlayerCharacterAdapter.CurrentContext;
			SelectWeapon(context);

			foreach (var ability in abilitiesController.AbilityControllers)
			{
				abilitiesController.ConnectAbilityToPlayer(context, ability.Key, true);
				var entityAction = ability.Value.GetEntityActionController().ActionKey.ToAxisName();
				var vertAxis = CF2Input.activeRig.axes.Get(entityAction);
				for (int i = 0; i < 2; i++)
				{
					await UniTask.Delay(250, cancellationToken: CancellationToken);
					vertAxis.SetNormalizedDelta(1);
					vertAxis.SetDigital();
				}
				await UniTask.Delay(550, cancellationToken: CancellationToken);
			}

			await UniTask.Delay(500, cancellationToken: CancellationToken);
		});
	}

}