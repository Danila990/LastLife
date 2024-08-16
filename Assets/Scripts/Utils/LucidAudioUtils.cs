using AnnulusGames.LucidTools.Audio;

namespace Utils
{
	public static class LucidAudioUtils
	{
		public static void IsActiveStop(this AudioPlayer player, float fadeOutDuration = 0.1f)
		{
			if(player == null || player.state == AudioPlayer.State.Stop)
				return;
			
			player.Stop(fadeOutDuration);
		}
	}
}
