using UnityEngine;
using UnityEngine.AI;

namespace Utils
{
	public static class NavMeshUtils
	{
		public static bool ProjectOnNavMesh(Vector3 point, float radius, out Vector3 projectedPoint)
		{
			var result = NavMesh.SamplePosition(point, out var hit, radius, NavMesh.AllAreas);
			projectedPoint = hit.position;
			return result;
		}
	}
}
