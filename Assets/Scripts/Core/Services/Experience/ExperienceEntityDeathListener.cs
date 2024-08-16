using System;
using Core.Entity;
using Core.Entity.Repository;
using MessagePipe;
using VContainer.Unity;

namespace Core.Services.Experience
{
	public class ExperienceEntityDeathListener : IInitializable, IDisposable
	{
		private readonly ISubscriber<MessageEntityDeath> _onEntityDeathSub;
		private readonly IPublisher<ExperienceMessage> _experiencePub;
		private IDisposable _disposable;

		public ExperienceEntityDeathListener(
			ISubscriber<MessageEntityDeath> onEntityDeathSub, 
			IPublisher<ExperienceMessage> experiencePub)
		{
			_onEntityDeathSub = onEntityDeathSub;
			_experiencePub = experiencePub;
		}
		
		public void Initialize()
		{
			_disposable = _onEntityDeathSub.Subscribe(OnDeath);
		}
		
		private void OnDeath(MessageEntityDeath obj)
		{
			if (obj.DiedArgs.SelfEntity is IExperienceEntity experienceEntity)
			{
				_experiencePub.Publish(new ExperienceMessage(experienceEntity.ExperienceCount, obj.DiedArgs.SelfEntity, obj.DiedArgs.DiedFrom, experienceEntity.GetExpPosition()));
			}
		}

		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}