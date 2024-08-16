using System;
using UnityEngine;

namespace Core.Actions
{
	[Serializable]
	public class ItemEntityActionController : IEntityActionController, IEntityActionData
	{
		[SerializeField] private AbstractScriptableEntityAction _abstractEntityAction;
		
		[field:SerializeField] public ActionKey ActionKey { get; set; }
		[field:SerializeField] public string ActionName { get; private set; }
		[field:SerializeField] public Sprite Icon { get; private set; }

		public bool SpecialAbility { get; set; }
		
		public ItemEntityActionController(
			AbstractScriptableEntityAction abstractEntityAction,
			ActionKey actionKey,
			string actionName,
			Sprite icon
		)
		{
			ActionKey = actionKey;
			_abstractEntityAction = abstractEntityAction;
			ActionName = actionName;
			Icon = icon;
			SpecialAbility = true;
		}


		public IEntityActionData ActionData => this;
		public IEntityAction EntityAction => _abstractEntityAction;
	}
}