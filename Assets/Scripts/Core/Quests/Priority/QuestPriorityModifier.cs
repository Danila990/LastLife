using System;
using System.Collections.Generic;
using UniRx;
using VContainer.Unity;

namespace Core.Quests.Priority
{
	public interface IQuestPriorityModifier
	{
		public IReactiveCommand<ModifiedPriorityArgs> OnPriorityChanged { get; }
	}
	
	public abstract class QuestPriorityModifier : IDisposable, IPostInitializable, IQuestPriorityModifier
	{
		protected readonly IQuestService QuestService;
		protected readonly Dictionary<string, ModifiedPriorityArgs> ModifiedPriorities;
		private readonly ReactiveCommand<ModifiedPriorityArgs> _onPriorityChanged;

		public IReactiveCommand<ModifiedPriorityArgs> OnPriorityChanged => _onPriorityChanged;

		protected QuestPriorityModifier(IQuestService questService)
		{
			ModifiedPriorities = new();
			_onPriorityChanged = new();
			
			QuestService = questService;
		}

		public abstract void PostInitialize();

		protected void Notify(in ModifiedPriorityArgs args)
		{
			if(_onPriorityChanged.IsDisposed)
				return;
			
			_onPriorityChanged.Execute(args);
		}
		
		public virtual void Dispose()
		{
			_onPriorityChanged?.Dispose();
		}
	}

}
