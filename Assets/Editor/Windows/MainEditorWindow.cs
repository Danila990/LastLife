using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.Windows
{
	public class MainEditorWindow : OdinMenuEditorWindow 
	{
		private List<BasePageEditor> _allPages;
		private MerchantItemsPage _merchantItems;

		[MenuItem("Tools/Main _%#T")]
		public static void OpenWindow()
		{
			GetWindow<MainEditorWindow>().Show();
		}
		
		

		protected override OdinMenuTree BuildMenuTree()
		{
			_allPages = new List<BasePageEditor>();
			
			var tree = new OdinMenuTree
			{
				Selection =
				{
					SupportsMultiSelect = false
				}
			};
			
			_merchantItems = new MerchantItemsPage();
			_allPages.Add(_merchantItems);
			
			tree.Add("Merchant", _merchantItems);
			
			
			foreach (var item in _allPages)
			{
				item.Init();
			}
			return tree;
		}
		
		protected override void OnImGUI()
		{
			base.OnImGUI();
			var condition = Event.current.type == EventType.KeyUp
			                && Event.current.modifiers == EventModifiers.Control
			                && Event.current.keyCode == KeyCode.S;

			if (condition)
			{
				foreach (var item in _allPages)
				{
					item.Save();
				}
				foreach (var item in _allPages)
				{
					item.Init();
				}
				Debug.Log("Save configs complete");
			}
		}
	}

}