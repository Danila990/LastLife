//
// using System.Collections.Generic;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace Editor
// {
// 	using UnityEditor;
// 	using UnityEngine.TestTools;
//
// 	[InitializeOnLoad, ExcludeFromCoverage]
// 	public static class PageSelectingGUI
// 	{
// 		private static Queue<Object> _queue;
// 		private const int MaxQueueCount = 5;
// 		private static Object _lastObject;
// 		static PageSelectingGUI()
// 		{
// 			_queue = new Queue<Object>();
// 			Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
// 			Selection.selectionChanged += SelectionChanged;
// 		}
// 		
// 		private static void SelectionChanged()
// 		{
// 			if (!Selection.activeObject || Selection.activeObject == _lastObject)
// 				return;
// 			
// 			if (_queue.Count > MaxQueueCount)
// 			{
// 				_queue.Dequeue();
// 			}
// 			if (_lastObject != null)
// 			{
// 				_queue.Enqueue(_lastObject);
// 			}
// 			_lastObject = Selection.activeObject;
// 		}
//
// 		private static void SelectionMoveDown()
// 		{
// 			if (_queue.TryDequeue(out var obj))
// 			{
// 				Selection.activeObject = obj;
// 			}
// 		}
//
// 		private static void OnPostHeaderGUI(Editor editor)
// 		{
// 			var totalRect = EditorGUILayout.GetControlRect();
// 			var controlRect = EditorGUI.PrefixLabel(totalRect, EditorGUIUtility.TrTempContent("Page"));
// 			if (GUI.Button(controlRect,"<- Back"))
// 			{
// 				SelectionMoveDown();
// 			}
// 		}
// 	}
// }