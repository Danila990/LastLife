using Analytic;
using Core.Entity.Repository;
using Core.Fuel;
using UnityEngine;
using VContainer;

namespace Core.Carry
{
	public class FuelKanisterContext : CarriedContext
	{
		[SerializeField] private FuelTank _fuelTank;

		[Inject] private readonly IFuelService _fuelService;
		[Inject] private readonly IEntityRepository _entityRepository;
		[Inject] private readonly IAnalyticService _analyticService;

		protected override void OnAttachInternal()
		{
			_analyticService.SendEvent($"OilRig:Minigame:TakeFuelCanister");
			_fuelService.AddTank(_fuelTank);
			OnDestroyed(_entityRepository);
			Destroy(gameObject);
		}
	}
}
