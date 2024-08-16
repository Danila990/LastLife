using Core.Fuel;
using UniRx;
using Utils;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.TicketUI
{
	public class FuelTanksCountController : UiController<FuelTankCountView>, IStartable
	{
		private readonly IFuelService _fuelService;
    
		public FuelTanksCountController
		(
			IFuelService fuelService
		)
		{
			_fuelService = fuelService;
		}
    
		public void Start()
		{
			_fuelService.TanksCount.SubscribeToText(View.Count).AddTo(View);
		}
	}
	
}
