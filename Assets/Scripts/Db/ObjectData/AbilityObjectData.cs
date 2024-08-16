using System;
using Core.Actions;

namespace Db.ObjectData
{
	[Serializable]
	public class AbilityObjectData : ObjectData
	{
		public AbstractScriptableEntityAction TargetAction;
	}
}