using System;
using Core.Entity.Ai.AiItem.Data;

namespace Core.Entity.Ai.AiItem
{
	public interface IAiItem : IDisposable
	{
		uint ItemUid { get; }
		bool InUse { get; }
		float UseRange { get; }
		float UseActionDuration { get; }
		IAiItemData AiItemData { get; }
		void Use(IAiTarget bTreeTarget);
		void Tick(ref float deltaTime);
		float GetPriority(IAiTarget aiTarget);
		bool IsValidItem(IAiTarget aiTarget);
		void OnAnimEvent(object args);
		void EndUse(bool success);
		void Disable();
	}

	public interface IAiItemUseListener
	{
		void OnUse(IAiItem aiItem, IAiTarget aiTarget);
		void OnEndUse(IAiItem aiItem);
	}
}