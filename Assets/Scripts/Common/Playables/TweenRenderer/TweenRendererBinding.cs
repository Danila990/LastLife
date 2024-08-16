using UnityEngine;

namespace Common.Playables.TweenRenderer
{
	public class TweenRendererBinding : MonoBehaviour
	{
		public Renderer Renderer;
		public string Property;
		public MaterialsDisposable Disposable;
		public string Key;

		public MaterialsDisposable GetMaterials()
		{
			Disposable?.Restore();
			Disposable = null;
			if (Renderer.sharedMaterials.Length == 1)
			{
				var material = new Material(Renderer.sharedMaterial);

				Disposable = new MaterialsDisposable()
				{
					Rend = Renderer,
					materials = new Material[] { material }
				};
			}
			else
			{
				Disposable = new MaterialsDisposable()
				{
					Rend = Renderer,
					materials = Renderer.sharedMaterials
				};
			}
			return Disposable;
		}
	}

	public class MaterialsDisposable
	{
		public Renderer Rend;
		public Material[] materials;
		public Material[] materialsCopy;
		
		public void Restore()
		{
			if (materials.Length > 1)
			{
				Rend.sharedMaterials = materials;
			}
			else
			{
				Rend.sharedMaterial = materials[0];
			}
		}
	}

}