using System;
using System.Collections.Generic;
using SharedUtils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Utils;

namespace Core.Entity.Step
{
	[CreateAssetMenu(menuName = SoNames.FOOTSTEPS + nameof(FootstepData), fileName = nameof(FootstepData))]
	public class FootstepData : SerializedScriptableObject
	{
		[SerializeField] private FootstepSettings _fallback;
		[OdinSerialize]
		[SerializeField]
		private Dictionary<PhysicMaterial, FootstepSettings> _footsteps;
		
		public (FootstepSettings Settings, AudioClip Clip) GetClip(PhysicMaterial material)
		{
			if (_footsteps == null || !material)
			{
				return (_fallback, _fallback.Clips.GetRandom());
			}
			
			return _footsteps.TryGetValue(material, out var settings) ? (settings, settings.Clips.GetRandom()) : (_fallback, _fallback.Clips.GetRandom());
		}
	}

	[Serializable]
	public class FootstepSettings
	{
		public float MaxDistance = 500f;
		public float Volume = 1f;
		public float Spread = 1f;
		public float Pitch = 1f;
		public AudioRolloffMode Mode = AudioRolloffMode.Logarithmic;

		public AudioClip[] Clips;
	}
}
