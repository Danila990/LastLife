using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Boosts.Impl
{
	public class CompositeBoostHandler : IDisposable
	{
		private readonly Dictionary<string, IEndBoostHandler> _endHandlers = new();
		public IBoost Boost { get; set; }

		public IBoostImpl BoostImpl { get; set; }

		public IReadOnlyDictionary<string, IEndBoostHandler> EndHandlers => _endHandlers;

		public void AddEndHandler(IEndBoostHandler endBoostHandler)
		{
			_endHandlers.TryAdd(endBoostHandler.Type, endBoostHandler);
		}

		public void Dispose()
		{
			foreach (var endHandler in _endHandlers.Values)
			{
				endHandler?.Dispose();
			}
		}


		public override string ToString()
		{
			return $"Composite Handler\n)Boost: {Boost?.AdditionalName} | {Boost?.BoostArgs.Type}\nBoostImpl {BoostImpl?.AdditionalName} | {BoostImpl?.BoostArgs}";
		}

		public TBoostHandler GetEndHandler<TBoostHandler>() where TBoostHandler : class, IEndBoostHandler
		{
			return _endHandlers.Values.First(x => x is TBoostHandler) as TBoostHandler;
		}
	}
}
