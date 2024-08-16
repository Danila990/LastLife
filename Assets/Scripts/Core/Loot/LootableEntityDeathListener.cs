using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Core.Entity.Head;
using Core.Entity.Repository;
using MessagePipe;
using SharedUtils;
using UnityEngine;
using Utils;
using VContainer.Unity;

namespace Core.Loot
{
	public class LootableEntityDeathListener : IInitializable, IDisposable
	{
		private readonly ISubscriber<MessageEntityDeath> _onEntityDeathSub;
		private readonly IPublisher<LootMessage> _lootPub;
		private IDisposable _disposable;
		
		public LootableEntityDeathListener(
			ISubscriber<MessageEntityDeath> onEntityDeathSub, 
			IPublisher<LootMessage> lootPub)
		{
			_onEntityDeathSub = onEntityDeathSub;
			_lootPub = lootPub;
		}
		
		public void Initialize()
		{
			_disposable = _onEntityDeathSub.Subscribe(OnDeath);
		}
		
		private void OnDeath(MessageEntityDeath obj)
		{
			if (obj.DiedArgs.SelfEntity is ILootEntity lootEntity)
			{
				_lootPub.Publish(new LootMessage(ParseLootData(lootEntity.LootData), CalcPositions(lootEntity)));
			}
		}

		private static string[] ParseLootData(LootData lootData)
		{
			IEnumerable<string> lootIds = lootData.GuaranteedLoot;
			var randomIds = new string[lootData.RandomQuantity];

			for (var i = 0; i < lootData.RandomQuantity; i++)
				randomIds[i] = lootData.RandomLoot.GetRandom();

			return lootIds.Concat(randomIds).ToArray();
		}
		
		private static Vector3[] CalcPositions(ILootEntity entity)
		{
			var lootData = entity.LootData;
			var position = entity.GetPosition();
			
			var count = lootData.GuaranteedLoot.Length + lootData.RandomQuantity;
			var points = new Vector3[count];
			MathUtils.GetPointsAroundOriginAsArray(position, ref points);
			
			for (var i = 0; i < points.Length; i++)
			{
				if (NavMeshUtils.ProjectOnNavMesh(points[i], 2f, out var point))
				{
					points[i] = point;
				}
			}
			
			return points;
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
	
	public struct LootMessage
	{
		public readonly string[] LootIds;
		public readonly Vector3[] Positions;

		public LootMessage(string[] lootIds, Vector3[] positions)
		{
			LootIds = lootIds;
			Positions = positions;
		}
	}
}