using System;
using AnnulusGames.LucidTools.Audio;
using Core.Boosts.Impl;
using Core.Entity.Characters;
using Core.HealthSystem;
using SharedUtils;
using UnityEngine;

namespace Core.Effects
{
	public class CharacterEffector : BaseCharacterEffector
	{
		protected override (string, Effect) DoEffectInternal(EffectArgs args)
		{
			var behaviour = EffectsDataSo.GetEffect(args.EffectType);
			//TODO: STOP HERE
			return VisualizeEffect(behaviour, args.Duration, args.EffectType.ToString());
		}

		private (string, Effect) VisualizeEffect(EffectData data, float duration, string id)
		{
			if (VfxFactory.TryGetParticle(data.ParticleKey, out var vfxContext))
			{
				AudioPlayer audioPlayer = null;

				void Start()
				{
					if (!CurrentContext)
						return;

					if (data.Sound)
					{
						audioPlayer = LucidAudio.PlaySE(data.Sound)
							.SetPosition(CurrentContext.MainTransform.position)
							.SetVolume(0.2f)
							.SetSpatialBlend(data.SpatialBlend);
					}

					var meshProvider = (CharacterMeshProvider)CurrentContext.MeshProvider;
					var mainRenderer = CurrentContext.MeshProvider.MainRenderer;
					if (mainRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
					{
						var shape = vfxContext.ParticleSystem.shape;
						shape.enabled = true;
						shape.useMeshColors = false;
						//shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
						shape.skinnedMeshRenderer = skinnedMeshRenderer;
					}

					var follower = new TransformFollower(vfxContext.transform, meshProvider.GetBone(data.BoneToAttach));
					follower.Cancel(vfxContext.ParticleSystem.main.duration).Forget();
                    vfxContext.transform.rotation = Quaternion.identity;
                    vfxContext.Play();
				}

				void End()
				{
					if (!CurrentContext)
						return;
					
					audioPlayer?.Stop();
					RemoveEffect(id);
					var shape = vfxContext.ParticleSystem.shape;
					shape.shapeType = ParticleSystemShapeType.Rectangle;
					vfxContext.Release();
					VfxFactory.Release(data.ParticleKey, vfxContext);
				}

				var effect = new Effect(Start, End, duration, data.Type);
				effect.Start();
				return (id, effect);
			}

			return Empty;
		}

		protected override void OnBoostApplied(AppliedBoostArgs appliedBoostArgs)
		{
			if (_activeEffects.TryGetValue(appliedBoostArgs.BoostArgs.Type, out var timer))
			{
				timer.Renew();
				return;
			}
			
			var data = EffectsDataSo.GetBoostEffect(in appliedBoostArgs.BoostArgs);
			if (data.ApplySound is { Length: > 0 })
			{
				LucidAudio.PlaySE(data.ApplySound.GetRandom())
					.SetPosition(CurrentContext.MainTransform.position)
					.SetVolume(0.5f)
					.SetSpatialBlend(data.SpatialBlend);
			}
			var effect = VisualizeEffect(data, appliedBoostArgs.BoostArgs.Duration, appliedBoostArgs.BoostArgs.Type);
			if (effect != Empty)
			{
				_activeEffects.Add(effect.Item1, effect.Item2);
				_effects.Add(effect.Item2);
				_onEffectStart.Execute(effect.Item2.EffectType);
			}
		}

		protected override void OnBoostRemoved(BoostArgs boostArgs)
		{
			RemoveEffect(EffectsDataSo.GetBoostEffect(in boostArgs).BoostType);
		}
	}
}
