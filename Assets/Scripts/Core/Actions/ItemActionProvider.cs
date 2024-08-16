using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Core.Actions
{
	[Serializable]
	public class ItemActionProvider : IActionProvider
	{
		public ItemEntityActionController[] Actions;
		public ICollection<IEntityActionController> ActionControllers => EntityActions.Values;
		[ShowInInspector, HideInEditorMode]
		public IDictionary<ActionKey, IEntityActionController> EntityActions { get; private set; }
		
		public static Dictionary<ActionKey, IEntityActionController> ZeroDictionary = new Dictionary<ActionKey, IEntityActionController>(0);
		private readonly List<(ActionKey, bool)> _abilityKeys = new ();

		public void Initialize()
		{
			EntityActions = Actions.Length != 0 ? Actions.ToDictionary(controller => controller.ActionKey, controller => (IEntityActionController)controller) : ZeroDictionary;
		}
		
		public void AddAbility(IEntityActionController entityActionController, bool additionalAbility)
		{
			var nextAction = GetNextAction(additionalAbility);
			EntityActions[nextAction] = entityActionController;
			entityActionController.ActionKey = nextAction;
		}
		
		private ActionKey GetNextAction(bool additionalAbility)
		{
			if (additionalAbility)
			{
				var contains = _abilityKeys.Any(tuple => tuple.Item2);
				if (contains)
				{
					var tuple = _abilityKeys.First(x => x.Item2);
					return tuple.Item1;
				}
			}
			var nextAction = GetActionKeyFromContext(this);
			_abilityKeys.Add((nextAction, additionalAbility));
			return nextAction;
		}

		private static ActionKey GetActionKeyFromContext(ItemActionProvider itemActionProvider)
		{
			var max = itemActionProvider
				.EntityActions
				.Values
				.Max(controller => (int)controller.ActionKey);

			var resultAction = (ActionKey) (max + 1);
			return resultAction is ActionKey.AimButton ? ActionKey.ActionFour : resultAction;
		}
	}
	
	public interface IActionProvider
	{
		ICollection<IEntityActionController> ActionControllers { get; }
		void AddAbility(IEntityActionController entityActionController, bool additionalAbility);
	}
}