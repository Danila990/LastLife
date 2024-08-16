using Sirenix.OdinInspector;
using UnityEngine;
public class Equipmentizer : MonoBehaviour
{
	[SerializeField] private bool _multipleMesh;
	[HideIf("_multipleMesh")] public SkinnedMeshRenderer SelfMeshRenderer;
	[ShowIf("_multipleMesh")] public SkinnedMeshRenderer[] SelfMeshRenderers;
	
	[Button]
	private void FindMesh(string rName = "")
	{
		var renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (_multipleMesh)
		{
			SelfMeshRenderers = renderers;
			return;
		}
		
		foreach (var renderer in renderers)
		{
			if (renderer.name.Contains(rName))
			{
				SelfMeshRenderer = renderer;
				return;
			}
		}

		SelfMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
	}
	
	
	[Button]
	public void AttachTo(SkinnedMeshRenderer targetRenderer)
	{
		if (_multipleMesh)
		{
			foreach (var selfMeshRenderer in SelfMeshRenderers)
			{
				selfMeshRenderer.bones = targetRenderer.bones;
			}
			return;
		}
		SelfMeshRenderer.bones = targetRenderer.bones;
	}
}
