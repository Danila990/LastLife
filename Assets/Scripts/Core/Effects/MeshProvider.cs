using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Effects
{
	public class MeshProvider : MonoBehaviour
	{
		public Renderer MainRenderer;
		public Renderer[] AllRenderers;
		
		#if UNITY_EDITOR

		[Button]
		private void GetRenderers()
		{
			MainRenderer = GetComponentInChildren<Renderer>();
			AllRenderers = GetComponentsInChildren<Renderer>();
		}
		
		#endif
	}


}
