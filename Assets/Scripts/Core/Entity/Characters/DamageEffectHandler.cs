using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Core.HealthSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effects
{
    public class DamageEffectHandler : BaseEffectAnimator
    {
        private Dictionary<EffectType, (DamageEffect, AudioClip)> _damageEffects;
        private EffectsDataSo _dataso;
        private IHealth _health;
        private Transform _spawnAudioPoint;
        private AudioPlayer _audioPlayer;

        public void SetContext(EffectsDataSo dataso, IHealth health, Transform spawnAudioPoint)
        {
            _dataso = dataso;
            _health = health;
            _spawnAudioPoint = spawnAudioPoint;
            _damageEffects = new Dictionary<EffectType, (DamageEffect, AudioClip)>(5);
        }

        protected override void OnEffect(EffectType effectType, bool isStarted)
        {
            if (_health == null || _health.IsDeath)
                return;

            SetDamageEffect(effectType, isStarted);
        }

        private void SetDamageEffect(EffectType effectType, bool isStarted)
		{
            if (!isStarted && !effectType.Equals(EffectType.None))
                return;

            if (!_damageEffects.ContainsKey(effectType))
            {
                EffectData effectData = _dataso.GetEffect(effectType);
                if (!effectData.IsEffectDamage)
                    return;
                DamageEffect damageEffect = new DamageEffect(effectData.Damage, effectData.CountTick, effectData.TickDelay, _health, End);
                _damageEffects.Add(effectType,(damageEffect, effectData.SoundEffect));
            }

            var data = _damageEffects[effectType];
            if (data.Item2)
            {
                _audioPlayer = LucidAudio
                 .PlaySE(data.Item2)
                  .SetPosition(_spawnAudioPoint.position)
                  .SetVolume(0.5f)
                  .SetSpatialBlend(1);
            }
            data.Item1.Renew();
        }

        private void End()
        {
            _audioPlayer?.Stop();
        }

        public void Dispose()
        {
            foreach (var item in _damageEffects.Values)
            {
                item.Item1.Dispose();
            }
        }
    }
}