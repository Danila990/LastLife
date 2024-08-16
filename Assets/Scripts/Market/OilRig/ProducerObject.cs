using Core.Carry;
using Core.Factory;
using Core.Quests.Messages;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Market.OilRig
{
	public class ProducerObject : MonoBehaviour
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.AllIds")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[SerializeField] private string _spawnedObjectId;
		[SerializeField] private Transform _spawnPoint;
		
		[SerializeField] public ConveyorObject _conveyor;
		
		[Inject] private readonly IObjectFactory _objectFactory;
		[Inject] private readonly IQuestMessageSender _questMessageSender;
		public ConveyorObject ConveyorObject => _conveyor;

		public void Produce()
		{
			var instance = (CarriedContext)_objectFactory.CreateObject(_spawnedObjectId, _spawnPoint.position, _spawnPoint.rotation);
			_questMessageSender.SendProduceMessage(_spawnedObjectId, 1);
			_conveyor.Place(instance);
		}
	}
}
