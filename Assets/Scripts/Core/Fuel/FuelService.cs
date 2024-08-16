using System;
using System.Collections.Generic;
using SharedUtils.PlayerPrefs;
using UniRx;

namespace Core.Fuel
{
	public interface IFuelService
	{
		IReactiveProperty<int> TanksCount { get; }
		void AddTank(FuelTank tank);
		FuelTank GetTank();
		bool HasFuel();
	}
	
	public class FuelService : IFuelService
	{
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly Stack<FuelTank> _tanks = new Stack<FuelTank>();
		private readonly IntReactiveProperty _tanksCount ;

		public IReactiveProperty<int> TanksCount => _tanksCount;
		private const string TANK_COUNT_KEY = "TanksCount";
		
		public FuelService(IPlayerPrefsManager playerPrefsManager)
		{
			_playerPrefsManager = playerPrefsManager;
			_tanksCount = new IntReactiveProperty(_playerPrefsManager.GetValue<int>(TANK_COUNT_KEY, 0));
			for (int i = 0; i < _tanksCount.Value; i++)
			{
				_tanks.Push(new FuelTank { Fuel = 20 } );
			}
		}
		
		public void AddTank(FuelTank tank)
		{
			_tanks.Push(tank);
			_tanksCount.Value = _tanks.Count;
			_playerPrefsManager.SetValue<int>(TANK_COUNT_KEY, _tanksCount.Value);
		}

		public FuelTank GetTank()
		{
			if (!HasFuel())
				return default(FuelTank);
			
			_tanksCount.Value--;
			_playerPrefsManager.SetValue<int>(TANK_COUNT_KEY, _tanksCount.Value);
			return _tanks.Pop();
		}

		public bool HasFuel() => _tanksCount.Value > 0;
	}
	
	[Serializable]
	public struct FuelTank
	{
		public float Fuel;
	}
}
