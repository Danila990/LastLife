using AnnulusGames.LucidTools.Audio;
using UnityEngine;

namespace Utils
{
	public static class LucidAudioUtil
	{
		public static void SetPositionSafe(this AudioPlayer audioPlayer, Vector3 position)
		{
			if (audioPlayer != null && audioPlayer.state != AudioPlayer.State.Stop)
			{
				audioPlayer.SetPosition(position);
			}
		}
	}
}