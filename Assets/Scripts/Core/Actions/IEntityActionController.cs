using System;
using Core.Entity;
using UniRx;
using UnityEngine;

namespace Core.Actions
{
	public interface IEntityActionController
	{
		ActionKey ActionKey { get; set; }
		IEntityActionData ActionData { get; }
		IEntityAction EntityAction { get; }
	}
	
	public interface IEntityAction
	{
		string Id { get; }
		void OnDeselect();
		void OnInput(bool state);
		void OnInputUp();
		void OnInputDown();
		void SetContext(EntityContext context);
		EntityContext CurrentContext { get; }
	}

	public interface IActionWithCooldown : IEntityAction
	{
		IObservable<float> OnCooldown { get; }
	}
	
	public interface IUnlockableAction : IEntityAction
	{
		IReactiveProperty<bool> IsUnlocked { get; set; }
	}
	
	public interface IEnableAction : IEntityAction
	{
		IReactiveProperty<bool> IsEnabled { get; set; }
	}

	public interface IAnimationEntityAction : IEntityAction
	{
		AnimationClip Clip { get; }
		void UseFromEvent();
	}

	public interface IEntityActionData
	{
		string ActionName { get; }
		Sprite Icon { get; }
	}

	public enum ActionKey
	{
		None = -1,
		MainAction = 0,
		ActionOne = 1,
		ActionTwo = 2,
		ActionThird = 3,
		AimButton = 4,
		ActionFour = 5
	}
}