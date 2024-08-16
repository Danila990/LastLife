using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Inventory
{
	public class ItemModel : MonoBehaviour
	{
		[TitleGroup("Grip"), SerializeField] private Transform _firstGripPoint;
		[TitleGroup("Grip"), SerializeField] private Transform _secondGripPoint;
		public Transform FirstGripPoint => _firstGripPoint;
		public Transform SecondGripPoint => _secondGripPoint;
		public Transform Origin;
		
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			if (_firstGripPoint)
			{
				Gizmos.DrawLine(_firstGripPoint.position-_firstGripPoint.forward * 0.5f,_firstGripPoint.position+_firstGripPoint.forward * 0.2f);
				Gizmos.DrawWireSphere(_firstGripPoint.position,0.05f);
			}
			Gizmos.color = Color.green;
			if (_secondGripPoint)
			{
				Gizmos.DrawLine(_secondGripPoint.position-_secondGripPoint.forward * 0.5f,_secondGripPoint.position+_secondGripPoint.forward * 0.2f);
				Gizmos.DrawWireSphere(_secondGripPoint.position,0.05f);
			}
		}
	}

}