using Core.Boosts.Impl;
using Core.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Boosts
{
	public class BoostTester : MonoBehaviour
	{
		[Inject] private readonly IPlayerSpawnService _spawnService;


		[Button]
		public void Add(BoostArgs args)
		{
			_spawnService.PlayerCharacterAdapter.BoostsInventory.Add(args);
		}
		
		[Button]
		public void Boost(string type)
		{
			_spawnService.PlayerCharacterAdapter.BoostProvider.ApplyBoost(type);
		}
	}
}
