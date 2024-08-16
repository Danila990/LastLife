using UnityEngine;

namespace Core.Map
{
	public interface IPlayerTeleportProvider
	{
		public Vector3 GetTpPoint(Vector3 currentPosition, float radius);
	}
}
