using CustEditor.Attributes;
using Sirenix.OdinInspector.Editor;
using Ui.Sandbox.WorldSpaceUI;
using UnityEditor;
using UnityEngine;

namespace Editor.CustomAttributes
{
	public class WorldUiNamesAttributeDrawer : OdinAttributeDrawer<WorldUiNamesAttribute, string>
	{
		private static WorldSpaceUISO _worldUi;
		private GenericMenu _menu;

		protected override void Initialize()
		{
			_menu = new GenericMenu();
			_worldUi ??= AssetDatabase.LoadAssetAtPath<WorldSpaceUISO>("Assets/Settings/Data/Ui/WorldSpaceUISO.asset");

			foreach (var nodeName in _worldUi.UI)
			{
				_menu.AddItem(new GUIContent(nodeName.Key), false, OnSelect, nodeName.Key);
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