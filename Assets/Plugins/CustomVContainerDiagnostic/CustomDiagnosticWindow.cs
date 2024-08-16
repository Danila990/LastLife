using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Diagnostics;
using VContainer.Unity;

namespace Plugins.CustomVContainerDiagnostic
{
	public class CustomDiagnosticWindow : OdinMenuEditorWindow
	{
		private List<RegistrationObjectData> _registrations;

		protected override OdinMenuTree BuildMenuTree()
		{
			var tree = new OdinMenuTree
			{
				Selection =
				{
					SupportsMultiSelect = false
				},
				
			};
			
			_registrations = ResolveObjects();
			if (_registrations == null)
				return tree;
			foreach (var registration in _registrations)
			{
				tree.Add($"{registration.ImplementationTypeNamespace}/{registration.Name}", registration.Object);
			}
			tree.DrawSearchToolbar();
			return tree;
		}
		
		[MenuItem("Tools/CustomVContainerDiagnostic")]
		public static void OpenWindow()
		{
			GetWindow<CustomDiagnosticWindow>().Show();
		}

		protected override void OnImGUI()
		{
			base.OnImGUI();
			MenuTree.DrawSearchToolbar();
			DrawMenuSearchBar = true;

			if (GUILayout.Button("Refresh"))
			{
				Refresh();
			}
		}

		private void Refresh()
		{
			ForceMenuTreeRebuild();
		}

		private List<RegistrationObjectData> ResolveObjects()
		{
			if (!VContainerSettings.DiagnosticsEnabled)
				return null;
			var infos = DiagnositcsContext.GetDiagnosticsInfos();
			var scope = LifetimeScope.Find<LifetimeScope>(SceneManager.GetActiveScene());
			var list = new List<RegistrationObjectData>();
			
			foreach (var info in infos)
			{
				try
				{
					var registeredObject = scope.Container.Resolve(info.ResolveInfo.Registration);
					list.Add(
						new RegistrationObjectData(
							info.ResolveInfo.Registration.ImplementationType.Namespace,
						info.ResolveInfo.Registration.ImplementationType.GetNiceName(),
						registeredObject));
				}
				catch (Exception e)
				{
					Debug.LogError(e);	
					throw;
				}
			}
			return list;
		}
		
		private readonly struct RegistrationObjectData
		{
			public string ImplementationTypeNamespace { get; }
			public readonly string Name;
			public readonly object Object;
			public RegistrationObjectData(string implementationTypeNamespace, string name, object o)
			{
				ImplementationTypeNamespace = implementationTypeNamespace;
				Name = name;
				Object = o;
			}
		}
	}
}