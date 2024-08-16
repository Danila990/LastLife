using System;
using Db.ObjectData;
using UniRx;

namespace Core.Actions.SpecialAbilities
{
	public class AbilityController : IDisposable
	{
		public readonly AbilityObjectData Data;
		private readonly BoolReactiveProperty _isUnlocked;
		private readonly ItemEntityActionController _cashedActionController;
		
		
		public AbilityController(AbilityObjectData data)
		{
			Data = data;
			_cashedActionController = new ItemEntityActionController(Data.TargetAction, ActionKey.ActionThird, Data.Name, Data.Ico);
			_isUnlocked = new BoolReactiveProperty();
			
			if (Data.TargetAction is IUnlockableAction unlockableAction)
			{
				unlockableAction.IsUnlocked = _isUnlocked;
			}
		}

		public void SetIsUnlocked(bool isUnlocked)
		{
			_isUnlocked.Value = isUnlocked;
		}

		public IEntityActionController GetEntityActionController()
		{
			return _cashedActionController;
		}
		
		public void Dispose()
		{
			_isUnlocked?.Dispose();
		}
	}
}