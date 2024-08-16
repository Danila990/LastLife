using AnnulusGames.LucidTools.Audio;
using Core.Entity;
using Core.Entity.Head;
using Core.HealthSystem;
using Db.VFXDataDto.Impl;
using UnityEngine;

namespace Core.Effects
{
	public class HeadEffector : LifeEntityEffector
    {
		private readonly Vector3 _shapeScale = Vector3.one * 1.2f;
		private const float FX_SCALE = 2f;


		private HeadContext _currentContext;
		
		public override void SetContext(LifeEntity context)
		{
			_currentContext = (HeadContext) context;
			base.SetContext(context);
		}

		protected override (string, Effect) DoEffectInternal(EffectArgs args)
		{
			var effectData = EffectsDataSo.GetEffect(args.EffectType);
			
			if (VfxFactory.TryGetParticle(effectData.ParticleKey, out var context))
			{
				AudioPlayer audioPlayer = null;
				var effect = new Effect(StartEffect, EndEffect, args.Duration, effectData.Type);
				effect.Start();
				
				void EndEffect()
				{
					audioPlayer?.Stop();
					RemoveEffect(args.EffectType.ToString());
					context.Release();
					VfxFactory.Release(effectData.ParticleKey, context);
				}
				
				void StartEffect()
				{
					if (effectData.Sound)
					{
						audioPlayer = LucidAudio.PlaySE(effectData.Sound)
							.SetPosition(_currentContext.MainTransform.position)
							.SetVolume(0.8f)
							.SetSpatialBlend(1);
					}

					var mainRenderer = _currentContext.MeshProvider.MainRenderer;

					SetEmitter(mainRenderer, context);
					context.ParticleSystem.transform.SetParent(mainRenderer.transform);
					context.Play();
				}
				return (args.EffectType.ToString(), effect);
			}
			
			return Empty;
		}

		private void SetEmitter(Renderer rend, VFXContext context)
		{
			if (rend is SkinnedMeshRenderer skinnedMeshRenderer)
			{
				var shape = context.ParticleSystem.shape;
				shape.enabled = true;
				shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
				shape.skinnedMeshRenderer = skinnedMeshRenderer;
				
				var main = context.ParticleSystem.main;
				main.startSizeMultiplier = FX_SCALE;
			}
			else if (rend is MeshRenderer meshRenderer)
			{
				var shape = context.ParticleSystem.shape;
				shape.enabled = true;
				shape.shapeType = ParticleSystemShapeType.MeshRenderer;
				shape.meshRenderer = meshRenderer;
				shape.scale = _shapeScale;

				var main = context.ParticleSystem.main;
				main.startSizeMultiplier = FX_SCALE;
			}

		}
	}
}
