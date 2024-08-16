using System;
using Cysharp.Threading.Tasks;
using Db.Quests;
using DG.Tweening;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;
using Ease = LitMotion.Ease;

namespace Ui.Sandbox.Quests.Views.Widgets
{
	public class QuestPopUpWidget : MonoBehaviour, IDisposable
	{
		[BoxGroup("SceneRefs")] [FormerlySerializedAs("_rectTransform")] [SerializeField]
		public RectTransform RectTransform;
		[BoxGroup("SceneRefs")] [SerializeField] 
		private Image _icon;
		[BoxGroup("SceneRefs")] [SerializeField] 
		private RectTransform _iconParent;
		[BoxGroup("SceneRefs")] [SerializeField]
		private TextMeshProUGUI _description;
		[BoxGroup("SceneRefs")] [SerializeField]
		private LayoutElement _descriptionElement;
		[BoxGroup("SceneRefs")] [SerializeField]
		private Image _checkImage;
		[BoxGroup("SceneRefs")] [SerializeField]
		private RectTransform _checkImageParent;
		[BoxGroup("SceneRefs")] [SerializeField]
		private LayoutGroup _layoutGroup;
		[BoxGroup("SceneRefs")] [SerializeField]
		private ContentSizeFitter _sizeFitter;
		
		[BoxGroup("Params")] [SerializeField] [LabelText("Max Description width")]
		private float _maxWidth;
		
		[BoxGroup("Animations")]
		[BoxGroup("Animations/Show|Hide")] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("CountTick")]
		public float MoveDuration;
		[BoxGroup("Animations/Show|Hide")] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("Delay Between")]
		public float DelayBetweenTweens;
		[BoxGroup("Animations/CheckImage")] [SerializeField] [HideIf("@_checkImage==null")] [HideLabel] [LabelText("CountTick")]
		private float _scaleDuration;
		[BoxGroup("Animations/CheckImage")] [SerializeField] [HideIf("@_checkImage==null")] [HideLabel] [LabelText("From")]
		private float _fromScale;
		[BoxGroup("Animations/CheckImage")] [SerializeField] [HideIf("@_checkImage==null")] [HideLabel] [LabelText("To")]
		private float _toScale;		
		[BoxGroup("Animations/CheckImage")] [SerializeField] [HideIf("@_checkImage==null")] [HideLabel] [LabelText("Ease")]
		private Ease _ease;
		[FormerlySerializedAs("_widthExtends")] [BoxGroup("Animations/RollUp")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("Extends")]
		private float _endWidth;
		[BoxGroup("Animations/RollUp")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("CountTick")]
		private float _rollUpDuration;
		[BoxGroup("Animations/Jump")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("CountTick")]
		private float _jumpDuration;
		[BoxGroup("Animations/Jump")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("EndScale")]
		private float _jumpEndScale;
		[BoxGroup("Animations/Jump")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("Force")]
		private float _jumpForce;
		[BoxGroup("Animations/Jump")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("AlphaGroup")]
		private CanvasGroup _canvasGroup;
		[BoxGroup("Animations/Jump")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("AlphaDuration")]
		private float _alphaDuration;
		[BoxGroup("Animations/Jump")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("JumpCurve")]
		private AnimationCurve _jumpCurve;
		[BoxGroup("Animations/Jump")] [SerializeField] [HideIf("@RectTransform==null")] [HideLabel] [LabelText("AlphaCurve")]
		private AnimationCurve _alphaCurve;
		
		private MotionHandle _moveHandle;
		private MotionHandle _scaleHandle;
		private MotionHandle _checkImageScaleHandle;
		private MotionHandle _fadeHandle;
		private MotionHandle _rollHandle;
		private Tween _jumpTween;
		

		public UniTask SetContent(in QuestMainData mainData)
		{
			_canvasGroup.alpha = 0;
			_icon.sprite = mainData.Icon;
			_description.text = mainData.Description;

			if (mainData.DisplayData.OverrideForWidget)
			{
				var h = _icon.rectTransform.sizeDelta.y;
				_icon.rectTransform.sizeDelta = new (mainData.DisplayData.OverridenWidgetWidth, h);
			}
			_icon.rectTransform.eulerAngles = mainData.DisplayData.Rotation;
			return CheckDescriptionOnOverflow();
		}

		private async UniTask CheckDescriptionOnOverflow()
		{
			_descriptionElement.preferredWidth = -1;
			await UniTask.NextFrame(destroyCancellationToken);
			if (_description.rectTransform.sizeDelta.x > _maxWidth)
			{
				_descriptionElement.preferredWidth = _maxWidth;
				LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
				await UniTask.NextFrame(destroyCancellationToken);
			}
			_canvasGroup.alpha = 1;
		}

		private void SetAnchorsToContent(AnchorPresets preset)
		{
			_checkImageParent.SetAnchor(preset);
			_iconParent.SetAnchor(preset);
			_description.rectTransform.SetAnchor(preset);
		}
		
		public MotionHandle DoAlpha(Vector2 offset, int startAlpha, float endAlpha)
		{
			_moveHandle.IsActiveCompleteCancel();
			
			_canvasGroup.alpha = startAlpha;
			RectTransform.anchoredPosition += offset;
			_moveHandle = LMotion
				.Create(startAlpha, endAlpha, MoveDuration)
				.Bind(ChangeGroupAlpha);

			return _moveHandle;
		}
		public MotionHandle DoScale(float startScale, float endScale)
		{
			_scaleHandle.IsActiveCompleteCancel();
			
			RectTransform.localScale = Vector3.one * startScale;
			_scaleHandle = LMotion
				.Create(startScale * Vector3.one, endScale * Vector3.one, MoveDuration)
				.BindToLocalScale(RectTransform);

			return _scaleHandle;
		}

		private void ChangeGroupAlpha(float value)
		{
			_canvasGroup.alpha = value;
		}
		
		public float JumpToPoint(Vector3 worldPosition)
		{
			_moveHandle.IsActiveCompleteCancel();
			_jumpTween?.Kill();
			_jumpTween = 
				RectTransform.DOJump(worldPosition, _jumpForce, 1, _jumpDuration)
				.Join(RectTransform.DOScale(_jumpEndScale, _jumpDuration))
				.Join(DOTween.To(()=> _canvasGroup.alpha, x=> _canvasGroup.alpha = x, 0, _alphaDuration).SetEase(_alphaCurve))
				.SetEase(_jumpCurve);
			return _jumpDuration;
		}

		public MotionHandle SetCheckImage(bool status)
		{
			if(!_checkImage)
				return _checkImageScaleHandle;
			
			_checkImageScaleHandle.IsActiveCompleteCancel();
			_fadeHandle.IsActiveCompleteCancel();
			
			if(!status)
			{
				_checkImage.gameObject.SetActive(false);
				return _checkImageScaleHandle;
			}

			_checkImageScaleHandle = LMotion.Create(_fromScale, _toScale, _scaleDuration)
				.WithEase(_ease)
				.Bind(ChangeScale);

			var fullAlpha = _checkImage.color;
			var zeroAlpha = fullAlpha;
			zeroAlpha.a = 0;
			_checkImage.color = zeroAlpha;
			
			_fadeHandle = LMotion.Create(zeroAlpha, fullAlpha, _scaleDuration)
				.WithEase(_ease)
				.BindToColor(_checkImage);
			
			_checkImage.gameObject.SetActive(true);
			return _checkImageScaleHandle;
		}

		private void OnDestroy()
		{
			Dispose();
		}

		public MotionHandle RollUp()
		{
			_sizeFitter.enabled = false;
			_layoutGroup.enabled = false;
			SetAnchorsToContent(AnchorPresets.TopRight);
			
			_rollHandle.IsActiveCompleteCancel();
			_rollHandle = LMotion.Create(RectTransform.rect.width, _endWidth, _rollUpDuration)
				.Bind(ChangeWidth);

			return _rollHandle;
		}

		public void ChangeWidth(float value) 
			=> RectTransform.sizeDelta = new Vector2(value, RectTransform.sizeDelta.y);
		private void ChangeScale(float value)
			=> _checkImage.transform.localScale = Vector3.one * value;
		
		public void Dispose()
		{
			_rollHandle.IsActiveCompleteCancel();
			_moveHandle.IsActiveCompleteCancel();
			_scaleHandle.IsActiveCompleteCancel();
			_checkImageScaleHandle.IsActiveCompleteCancel();
			_fadeHandle.IsActiveCompleteCancel();
			_jumpTween?.Kill();
			_jumpTween = null;
		}
	}
}
