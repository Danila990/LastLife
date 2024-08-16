using UnityEngine;
using VContainerUi.Interfaces;

namespace Tests.UITest.Custom.Conditions
{
	public class UiViewInactive<TView> : AUiViewCondition<TView> where TView : MonoBehaviour, IUiView
	{
		public UiViewInactive(bool projectScope) : base(projectScope)
		{
		}
		
		public override bool Satisfied()
		{
			var canvas = GetCanvas();
			if (canvas == null)
				return false;
			
			var views = canvas.gameObject.GetComponentsInChildren<TView>(true);
			var properLength = typeof(TView).IsAbstract || views.Length > 0 && views.Length < 2;
			return properLength && !views[0].gameObject.activeSelf;
		}

		public override string ToString()
		{
			return $"Inactive view({typeof(TView).Name})";
		}
	}
}