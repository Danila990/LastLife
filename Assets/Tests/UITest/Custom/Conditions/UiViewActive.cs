using UnityEngine;
using VContainerUi.Interfaces;

namespace Tests.UITest.Custom.Conditions
{
	public class UiViewActive<TView> : AUiViewCondition<TView> where TView : MonoBehaviour, IUiView
	{
		public UiViewActive(bool projectScope) : base(projectScope)
		{
		}
		
		public UiViewActive(Canvas canvas) : base(canvas)
		{
		}
		
		public override bool Satisfied()
		{
			var canvas = GetCanvas();
			if (canvas == null)
				return false;
			
			var views = canvas.gameObject.GetComponentsInChildren<TView>(false);
			return views.Length == 1 && views[0].gameObject.activeSelf;
		}

		public override string ToString()
		{
			return $"Active view({typeof(TView).Name})";
		}
	}
}