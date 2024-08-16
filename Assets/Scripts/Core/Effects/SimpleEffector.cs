using AnnulusGames.LucidTools.Audio;
using Core.Entity;
using Core.HealthSystem;
using UnityEngine;

namespace Core.Effects
{
	public class SimpleEffector : LifeEntityEffector
    {
		private LifeEntity _currentContext;
		
		public override void SetContext(LifeEntity context)
		{
			_currentContext = context;
			base.SetContext(context);
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
							.SetPosition(_currentContext.MainTransform.position)
							.SetVolume(0.5f)
							.SetSpatialBlend(1);
					}

					var mainRenderer = _currentContext.MeshProvider.MainRenderer;
					if (mainRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
					{
						var shape = context.ParticleSystem.shape;
						shape.enabled = true;
						shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
						shape.skinnedMeshRenderer = skinnedMeshRenderer;
					}
					context.ParticleSystem.transform.SetParent(mainRenderer.transform);
					context.Play();
				}
			}
			
			return Empty;
		}
	}

}
