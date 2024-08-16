using System.Collections.Generic;
using AnnulusGames.LucidTools.Audio;
using UnityEngine;
using VContainer.Unity;

namespace Core.Services
{
	public interface IAudioService
	{
		public bool TryPlayQueueSound(AudioClip clip, string source, float cooldown, out AudioPlayer player);
		public AudioPlayer PlayNonQueue(AudioClip clip);
	}

	public class AudioService : IAudioService, ITickable
	{
		private readonly List<PlayedAudio> _currentSounds = new();
		private readonly HashSet<string> _currentSoundHash = new();
		private readonly Stack<PlayedAudio> _toRemove = new();


		public bool TryPlayQueueSound(AudioClip clip, string source, float cooldown, out AudioPlayer player)
		{
			player = default;
			if (clip == null)
				return false;

			var key = clip.name + source;
			if (_currentSoundHash.Contains(key))
			{
				player = null;
				return false;
			}
			var played = new PlayedAudio
			{
				Key = key,
				Time = cooldown
			};
			_currentSounds.Add(played);
			_currentSoundHash.Add(key);
			player = LucidAudio.PlaySE(clip);
			return true;
		}

		public AudioPlayer PlayNonQueue(AudioClip clip)
		{
			return LucidAudio.PlaySE(clip);
		}

		public void Tick()
		{
			foreach (var sounds in _currentSounds)
			{
				sounds.Time -= Time.deltaTime;
				if (sounds.Time > 0) continue;
				_toRemove.Push(sounds);
			}

			while (_toRemove.TryPop(out var remove))
			{
				_currentSounds.Remove(remove);
				_currentSoundHash.Remove(remove.Key);
			}
		}
	}

	public class PlayedAudio
	{
		public string Key;
		public float Time;
	}
}