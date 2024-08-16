using System;
using VContainer.Unity;

namespace Core.Quests.Messages
{
	public interface IQuestMessageHandler<in TMessage> : IPostInitializable, IDisposable
	{
		public void Handle(TMessage msg);
	}
}
