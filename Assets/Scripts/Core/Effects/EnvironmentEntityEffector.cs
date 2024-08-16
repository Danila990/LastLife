using AnnulusGames.LucidTools.Audio;
using Core.HealthSystem;
using UnityEngine;

namespace Core.Effects
{
    public class EnvironmentEntityEffector : BaseEntityEffector
    {
        private IHealth _health;

        public void SetHealth(IHealth health)
        {
            _health = health;
        }

        public override void DoEffect(EffectArgs args)
        {
            if (_health.IsDeath)
                return;

            base.DoEffect(args);
        }

        protected override (string, Effect) DoEffectInternal(EffectArgs args)
        {

            var effectData = EffectsDataSo.GetEffect(args.EffectType);
            if (VfxFactory.TryGetParticle(effectData.ParticleKey, out var context))
            {
                AudioPlayer audioPlayer = null;

                var effect = new Effect(Start, End, args.Duration, args.EffectType);
                effect.Start();
                return (args.EffectType.ToString(), effect);
                
                void End()
                {
                    audioPlayer?.Stop();
                    RemoveEffect(args.EffectType.ToString());
                    context.Release();
                    VfxFactory.Release(effectData.ParticleKey, context);
                }

                void Start()
                {
                    if (effectData.Sound)
                    {
                        audioPlayer = LucidAudio.PlaySE(effectData.Sound)
                            .SetPosition(transform.position)
                            .SetVolume(0.5f)
                            .SetSpatialBlend(1);
                    }

                    var follower = new TransformFollower(context.transform, transform);
                    follower.Cancel(context.ParticleSystem.main.duration).Forget();
                    context.transform.rotation = Quaternion.identity;
                    context.transform.localScale = new Vector3(3, 1.5f, 3);
                    context.Play();
                }
            }

            return Empty;
        }
    }
}