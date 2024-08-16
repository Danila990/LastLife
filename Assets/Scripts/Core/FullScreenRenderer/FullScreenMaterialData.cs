using System;
using System.Linq;
using Core.FullScreenRenderer.MaterialAnimation;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.FullScreenRenderer
{
	public interface IFullScreenMaterialData
	{
		public bool TryGetMaterialData(string key, out FullScreenMaterial material);
	}
	
	[CreateAssetMenu(menuName = SoNames.FULLSCREEN_MATERIALS + nameof(FullScreenMaterialData), fileName = nameof(FullScreenMaterialData))]
	public class FullScreenMaterialData : ScriptableObject, IFullScreenMaterialData
	{
		[TableList]
		[SerializeField] private SerializedFullScreenMaterial[] _materials;

		public bool TryGetMaterialData(string key, out FullScreenMaterial fullScreenMaterial)
		{
			fullScreenMaterial = null;
			var data = _materials.FirstOrDefault(x => x.Key == key);
			if (data.Material == null || data.Animation == null)
				return false;

			fullScreenMaterial = new FullScreenMaterial(data.Material, data.Animation);
			return true;
		}
		
		[Serializable]
		private struct SerializedFullScreenMaterial
		{
			public string Key;
			public Material Material;
			public FullScreenMaterialAnimation Animation;
		}
	}

	public class FullScreenMaterial : IDisposable
	{
		public readonly Material MaterialPrefab;
		public readonly FullScreenMaterialAnimation Animation;

		public FullScreenMaterial(Material materialPrefab, FullScreenMaterialAnimation animation)
		{
			MaterialPrefab = materialPrefab;
			Animation = animation;
		}
		
		public void Dispose()
		{
			Animation.SetMaterialInstance(null);
		}
	}
}
