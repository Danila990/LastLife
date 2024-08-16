using Core.Factory;
using UnityEngine;

namespace Common.SpawnPoint
{
	public abstract class SpawnPoint : MonoBehaviour
	{
		public abstract void Create(IAdapterStrategyFactory strategyFactory);
	}
}