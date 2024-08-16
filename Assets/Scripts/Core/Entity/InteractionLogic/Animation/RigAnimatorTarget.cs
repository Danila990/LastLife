using UnityEngine;

namespace Core.Entity.InteractionLogic.Animation
{
	public class RigAnimatorTarget : MonoBehaviour
	{
		[Range(0,1f)] public float Weight;
		public Vector3 Position => transform.position;
	}
}