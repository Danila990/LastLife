using System.Collections.Generic;
using TweenPlayables;
using UnityEngine;
using UnityEngine.Playables;

namespace Common.Playables.TweenRenderer
{
	public sealed class TweenMaterialMixerBehaviour : TweenAnimationMixerBehaviour<TweenRendererBinding, RendererMaterialBehaviour>
	{
		readonly FloatValueMixer floatValueMixer = new();

		readonly Dictionary<string, MaterialsDisposable> materialDictionary = new();

		public override void OnPlayableDestroy(Playable playable)
		{
			foreach (var materials in materialDictionary.Values)
			{
				foreach (Material m in materials.materialsCopy)
				{
					Object.DestroyImmediate(m);
				}
				materials.Restore();
			}
			materialDictionary.Clear();
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			var renderer = playerData as TweenRendererBinding;
			if (renderer != null)
			{
				if (!materialDictionary.ContainsKey(renderer.Key))
				{
					var materials  = renderer.GetMaterials();
					materials.materialsCopy = new Material[materials.materials.Length];
					for (int i = 0; i < materials.materials.Length; i++)
					{
						materials.materialsCopy[i] = new Material(materials.materials[i]);
					}
					if (materials.materialsCopy.Length > 1)
					{
						materials.Rend.materials = materials.materialsCopy;
					}
					else
					{
						materials.Rend.material = materials.materialsCopy[0];
					}
					materialDictionary.Add(renderer.Key, materials);
				}
			}

			base.ProcessFrame(playable, info, playerData);
		}
		public override void Blend(TweenRendererBinding binding, RendererMaterialBehaviour behaviour, float weight, float progress)
		{
			floatValueMixer.TryBlend(behaviour.FloatTweenParameter, binding, weight, progress);
		}
		
		public override void Apply(TweenRendererBinding binding)
		{
			floatValueMixer.TryApplyAndClear(materialDictionary[binding.Key], (x, b) =>
			{
				foreach (var m in b.materialsCopy)
				{
					m.SetFloat(binding.Property, x);
				}
			});
		}
	}
}