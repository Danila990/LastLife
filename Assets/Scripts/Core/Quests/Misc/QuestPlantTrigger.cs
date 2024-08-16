using Core.Entity;
using Core.Quests.Messages;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Quests.Misc
{
	public class QuestPlantTrigger : MonoBehaviour
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.AllIds")]
		[InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, "")]
		[SerializeField] private string _factoryId;
		[SerializeField] private string _idToSend;
		
		[Inject] private IQuestMessageSender _questMessageSender;
		

		private void OnTriggerEnter(Collider other)
		{
			if (!other.TryGetComponent(out EntityContext context))
				return;
			
			if(context.SourceId == _factoryId)
				_questMessageSender.SendPlaceObjectMessage(_idToSend);
		}
	}
}
