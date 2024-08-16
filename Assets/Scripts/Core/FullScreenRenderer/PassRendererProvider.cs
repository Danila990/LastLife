using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer.Unity;

namespace Core.FullScreenRenderer
{
	public interface IPassRendererProvider
	{
		public void SetMaterial(Material material);
		public void ResetMaterial();
	}
	
	public class PassRendererProvider : IPostInitializable, IPassRendererProvider, IDisposable
	{
		private const string KEY = "FullScreenPassRendererFeature";
		private readonly IPassRendererData _rendererData;
		private FullScreenPassRendererFeature  _feature;

		public PassRendererProvider(IPassRendererData rendererData)
		{
			_rendererData = rendererData;
		}

		public void PostInitialize()
		{
			_feature = _rendererData.Data.rendererFeatures.FirstOrDefault(x => x.name == KEY) as FullScreenPassRendererFeature;
			
			if(!_feature)
				Debug.LogError($"{KEY} Not found");
		}
		
		public void SetMaterial(Material material)
		{
			if (material == null)
			{
				ResetMaterial();
				return;
			}
			
			_feature.passMaterial = material;
			_feature.SetActive(true);
		}

		public void ResetMaterial()
		{
			if(!_feature)
				return;
			
			_feature.SetActive(false);
			_feature.passMaterial = null;
		}

		public void Dispose()
		{
			ResetMaterial();
		}
	}
}
