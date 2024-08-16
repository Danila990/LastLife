using System.Collections.Generic;
using Core.Carry;
using Core.Factory;
using UniRx;

namespace Market.OilRig
{
	public interface IConveyorProvider
	{
		public void Register(ConveyorObject conveyor);
	}
	
	public class ConveyorProvider : IConveyorProvider
	{
		private readonly IConveyorSaveAdapter _saveAdapter;
		private readonly IObjectFactory _objectFactory;
		private readonly Dictionary<string, ConveyorObject> _conveyors;
		
		public IReadOnlyDictionary<string, ConveyorObject> Conveyor => _conveyors;
		
		public ConveyorProvider(IConveyorSaveAdapter saveAdapter, IObjectFactory objectFactory)
		{
			_saveAdapter = saveAdapter;
			_objectFactory = objectFactory;
			_conveyors = new();
		}
		
		public void Register(ConveyorObject conveyor)
		{
			if (_conveyors.TryAdd(conveyor.Id, conveyor))
			{
				conveyor.OnModified.Subscribe(OnModified).AddTo(conveyor);
				TryRestoreConveyor(conveyor);
			}
		}

		private void TryRestoreConveyor(ConveyorObject conveyor)
		{
			if (_saveAdapter.TryGetConveyorData(conveyor.Id, out var data))
			{
				foreach (var endPointData in data.EndPoints)
				{
					if (conveyor.CanPlaceIntoPoint(endPointData.EndPointId))
					{
						var instance = (CarriedContext)_objectFactory.CreateObject(endPointData.StoredFactoryId);
						conveyor.PlaceIntoPoint(instance, endPointData.EndPointId);
					}
				}
			}
		}

		private void OnModified(ConveyorObject conveyor)
		{
			_saveAdapter.Refresh(conveyor);
		}
	}
}