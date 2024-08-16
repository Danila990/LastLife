using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainerUi.Interfaces;

namespace Ui.Sandbox.SelectMenu
{
	public abstract class SelectMenuMainView : MonoBehaviour, IUiView
	{
		[TitleGroup("SelectMenuMainView", indent:true)]
		[FoldoutGroup("SelectMenuMainView/Easing")] [SerializeField] private Ease _easeShow = Ease.InQuad;
		[FoldoutGroup("SelectMenuMainView/Easing")] [SerializeField] private Ease _easeHide = Ease.OutQuad;
		[FoldoutGroup("SelectMenuMainView/Easing")] [SerializeField] private float _startShowScale = 0.8f;
		[TitleGroup("SelectMenuMainView/Buttons")] public Button HideButton;
		private MotionHandle _handle;
		private float _duration = 0.1f;
		public bool IsShow { get; private set; }

		protected void OnDisable()
		{
			DisableHandle();
		}

		private void DisableHandle() 
			=> _handle.IsActiveCancel();

		public void Show()
		{
			IsShow = true;
			InternalShow();
			transform.localScale = Vector3.one * _startShowScale;
			DisableHandle();
			_handle = LMotion
				.Create(Vector3.one * _startShowScale, Vector3.one, _duration)
				.WithEase(_easeShow)
				.BindToLocalScale(transform);
		}
		
		public void Hide()
		{
			IsShow = false;
			DisableHandle();
			_handle = LMotion
				.Create(transform.localScale, Vector3.one * _startShowScale, _duration)
				.WithEase(_easeHide)
				.WithOnComplete(InternalHide)
				.BindToLocalScale(transform);
		}

		public virtual void SetOrder(int index)
		{
			var parent = transform.parent;
			if (parent == null)
				return;
			var childCount = parent.childCount - 1;
			transform.SetSiblingIndex(childCount - index);
		}
		
		private void InternalHide()
		{
			gameObject.SetActive(false);
		}
		
		private void InternalShow()
		{
			gameObject.SetActive(true);
		}
		
		void IUiView.SetParent(Transform parent)
		{
			transform.SetParent(parent, false);
		}
		
		IUiElement[] IUiView.GetUiElements()
		{
			return gameObject.GetComponentsInChildren<IUiElement>();
		}

		void IUiView.Destroy()
		{
			DisableHandle();
			Destroy(gameObject);
		}
	}
}