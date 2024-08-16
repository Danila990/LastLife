using System;
using UniRx;

namespace Core.Boosts.Impl
{
	public interface IBoost
	{
		public string AdditionalName { get; }

		public BoostArgs BoostArgs { get; }
	}
	
	public interface IBoostImpl : IBoost
	{
		public IReactiveCommand<string> OnApply { get; }
		public IReactiveCommand<string> OnRemove { get; }
		
		public void ApplyEffect();
		public void RemoveEffect();
	}

	public interface IEndBoostHandler : IDisposable
	{
		public string Id { get; }
		public string Type { get; }

		public IReactiveCommand<string> OnBoostEnded { get; }
	}
}
