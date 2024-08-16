using Tests.UITest.Conditions;
using UnityEngine;
using VContainerUi.Interfaces;

namespace Tests.UITest.Custom.Conditions
{
	public abstract class AUiViewCondition<TView> : Condition<TView> where TView : MonoBehaviour, IUiView
	{
		private readonly Canvas _canvas;
		private readonly bool _projectScope;

		protected AUiViewCondition(bool projectScope)
		{
			_projectScope = projectScope;
		}
		
		protected AUiViewCondition(Canvas canvas)
		{
			_canvas = canvas;
		}
		
		protected Canvas GetCanvas()
		{
			if (_canvas)
			{
				return _canvas;
			}
			
			var canvases = Object.FindObjectsOfType<Canvas>();
			foreach (var canvas in canvases)
			{
				if (canvas.name.Contains("Tutorial") || canvas.name.Contains("DebugCanvas"))
					continue;
				
				var isProjectCanvas = canvas.name.Contains("Project");
				if (isProjectCanvas == _projectScope)
					return canvas;
			}
			return null;
		}
	}
}