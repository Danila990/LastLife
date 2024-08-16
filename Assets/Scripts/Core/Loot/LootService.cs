using System;
using System.Linq;
using Core.Factory;
using MessagePipe;
using UnityEngine.Assertions;
using VContainer.Unity;

namespace Core.Loot
{
	public interface ILootService
	{
		
	}
	
	public class LootService : ILootService, IInitializable, IDisposable
	{
		private readonly ISubscriber<LootMessage> _lootSub;
		private readonly ILootFactory _lootFactory;
		private IDisposable _disposable;

		public LootService(
			ISubscriber<LootMessage> lootSub,
			ILootFactory lootFactory
			)
		{
			_lootSub = lootSub;
			_lootFactory = lootFactory;
		}
		public void Initialize()
		{
			_disposable = _lootSub.Subscribe(OnLoot);
		}

		private void OnLoot(LootMessage msg)
		{
			var count = msg.Positions.Length;
			Assert.AreEqual(count, msg.LootIds.Length);
			
			for (int i = 0; i < count; i++)
			{
				_lootFactory.CreateObject(msg.LootIds[i], msg.Positions[i]);
			}
		}

		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}
