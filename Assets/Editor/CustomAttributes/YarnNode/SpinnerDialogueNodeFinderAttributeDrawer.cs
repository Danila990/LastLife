using CustEditor.Attributes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace Editor.CustomAttributes
{
	public class SpinnerDialogueNodeFinderAttributeDrawer : OdinAttributeDrawer<SpinnerDialogueNodeFinderAttribute, string>
	{
		private static YarnProject _yarnProject;
		private GenericMenu _menu;

		protected override void Initialize()
		{
			_menu = new GenericMenu();
			_yarnProject ??= AssetDatabase.LoadAssetAtPath<YarnProject>("Assets/Prefabs/Dialogue/MarketProject.yarnproject");
			
			foreach (var nodeName in _yarnProject.NodeNames)
			{
				_menu.AddItem(new GUIContent(nodeName), false, OnSelect, nodeName);
			}
		}
		
		private void OnSelect(object userdata)
		{
			ValueEntry.SmartValue = (string)userdata;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (EditorGUILayout.DropdownButton(new GUIContent(ValueEntry.SmartValue), FocusType.Keyboard))
			{
				_menu.ShowAsContext();
			}
		}
	}

}