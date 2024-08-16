using AnnulusGames.LucidTools.Audio;
using UnityEngine;

namespace Common
{
	public class AnimatorSoundEventListener : MonoBehaviour
	{
		public float Volume;
		public float Spread;
		public float MaxDistance;
		public float Pitch;
		public AudioRolloffMode Mode;
		
		public void PlaySound(Object unityObject)
		{
			if (unityObject is AudioClip audioClip)
			{
				LucidAudio
					.PlaySE(audioClip)
					.SetPosition(transform.position)
					.SetVolume(Volume)
					.SetSpatialBlend(1f)
					.SetSpread(Spread)
					.SetMaxDistance(MaxDistance)
					.SetPitch(Pitch)
					.SetRolloffMode(Mode);
			}
		}
	}
}