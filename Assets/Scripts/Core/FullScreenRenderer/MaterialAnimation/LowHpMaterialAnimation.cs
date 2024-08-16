using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using Utils.Constants;

namespace Core.FullScreenRenderer.MaterialAnimation
{
	[CreateAssetMenu(menuName = SoNames.FULLSCREEN_MATERIALS + nameof(LowHpMaterialAnimation), fileName = nameof(LowHpMaterialAnimation))]
	public class LowHpMaterialAnimation : FullScreenMaterialAnimation
	{
		[SerializeField, FoldoutGroup("FadeIn")] private float _fadeInDuration; 
		[SerializeField, FoldoutGroup("FadeIn")] private float _fadeInStartValue; 
		[SerializeField, FoldoutGroup("FadeIn")] private float _fadeInEndValue;
		[Space(20)]
		[SerializeField, FoldoutGroup("FadeOut")] private float _fadeOutDuration;
		[SerializeField, FoldoutGroup("FadeOut")] private float _fadeOutStartValue;
		[SerializeField, FoldoutGroup("FadeOut")] private float _fadeOutEndValue;
		[Space(20)]
		[SerializeField, FoldoutGroup("Fade")] private float _fadeDuration;
		[SerializeField, FoldoutGroup("Fade")] private float _fadeStartValue;
		[SerializeField, FoldoutGroup("Fade")] private float _fadeEndValue;

		private CompositeMotionHandle _compositeMotionHandle;
		
		
		protected async override UniTaskVoid FadeInInternalAwait(Action callback = null)
		{
			_compositeMotionHandle?.Cancel();
			_compositeMotionHandle = new CompositeMotionHandle();

			LMotion
				.Create(0f, 1f, _fadeInDuration)
				.Bind(SetFlash)
				.AddTo(_compositeMotionHandle);
			
			LMotion
				.Create(_fadeInStartValue, _fadeInEndValue, _fadeInDuration)
				.Bind(SetVignette)
				.AddTo(_compositeMotionHandle);
			
			LMotion
				.Create(Material.GetFloat(ShHash.NoiseSpeed_FullScreen1), 0, _fadeInDuration / 2)
				.Bind(SetNoiseSpeed)
				.AddTo(_compositeMotionHandle);

			await UniTask.Delay(_fadeInDuration.ToSec(), cancellationToken: Token);
			callback?.Invoke();
		}
		
		protected async override UniTaskVoid FadeOutInternalAwait(Action callback = null)
		{
			_compositeMotionHandle?.Cancel();
			_compositeMotionHandle = new CompositeMotionHandle();

			LMotion
				.Create(1f, 0f, _fadeOutDuration)
				.Bind(SetFlash)
				.AddTo(_compositeMotionHandle);

			LMotion
				.Create(_fadeOutStartValue, _fadeOutEndValue, _fadeOutDuration)
				.Bind(SetVignette)
				.AddTo(_compositeMotionHandle);
			
			await UniTask.Delay(_fadeOutDuration.ToSec(), cancellationToken: Token);
			callback?.Invoke();
		}
		
		protected async override UniTaskVoid FadeInternalAwait(Action callback = null)
		{
			_compositeMotionHandle?.Cancel();
			_compositeMotionHandle = new CompositeMotionHandle();
			
			LMotion
				.Create(0f, 1f, _fadeDuration)
				.WithLoops(2, LoopType.Yoyo)
				.Bind(SetFlash)
				.AddTo(_compositeMotionHandle);
			
			LMotion
				.Create(_fadeStartValue, _fadeEndValue, _fadeDuration)
				.WithLoops(2, LoopType.Yoyo)
				.Bind(SetVignette)
				.AddTo(_compositeMotionHandle);
			
			await UniTask.Delay((_fadeDuration * 2).ToSec(), cancellationToken: Token);
			callback?.Invoke();
		}
		
		public override void Release()
		{
			_compositeMotionHandle?.Cancel();
			_compositeMotionHandle = null;
		}
		
		private void SetVignette(float value) 
			=> Material.SetFloat(ShHash.VignettePower, value);

		private void SetFlash(float value)
			=> Material.SetFloat(ShHash.Flash, value);
		
		private void SetNoiseSpeed(float value)
			=> Material.SetFloat(ShHash.NoiseSpeed_FullScreen1, value);
	}
}
