using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Extensions;
using VContainerUi.Abstraction;

namespace Installer
{
	[CreateAssetMenu(menuName = SoNames.INSTALLERS + nameof(ScriptableUiViewInstaller), fileName = "UiViewInstaller")]
	public class ScriptableUiViewInstaller : ScriptableObjectInstaller
	{
		public ViewRegistration[] ViewRegistrations;
		public WindowRegistration[] WindowRegistrations;
		
		private Assembly _assemblyReference = null;
		
		public override void Install(IContainerBuilder builder)
		{
			foreach (var viewRegistration in ViewRegistrations)
			{
				var type = GetTypeFrom(viewRegistration);
				_assemblyReference ??= type.Assembly; 

				builder.Register(type, Lifetime.Singleton)
					.AsImplementedInterfaces().AsSelf();
			
				
				builder.Register((resolver) =>
					{
						var canvas = resolver.Resolve<Canvas>();
						var view = Instantiate(viewRegistration.View, canvas.transform);
						resolver.Inject(view);
						view.gameObject.SetActive(false);
						return view;
					}, viewRegistration.View.GetType(), Lifetime.Singleton)
					.AsImplementedInterfaces().As(viewRegistration.View.GetType());
			}
		
			foreach (var windowRegistration in WindowRegistrations)
			{
				var windowType = GetTypeFrom(windowRegistration, _assemblyReference);
				builder.Register(windowType, Lifetime.Singleton)
					.AsImplementedInterfaces().AsSelf();
			}
		}
		
		private static Type GetTypeFrom(ViewRegistration viewRegistration)
		{
			return viewRegistration.UseAssemblyResolve ? viewRegistration.View.GetType().Assembly.GetType(viewRegistration.ControllerType) :
				Type.GetType(viewRegistration.ControllerType);
		}
		
		private static Type GetTypeFrom(WindowRegistration viewRegistration, Assembly assemblyReference)
		{
			return viewRegistration.UseAssemblyResolve ? assemblyReference.GetType(viewRegistration.WindowType) :
				Type.GetType(viewRegistration.WindowType);
		}
	}
	
	[Serializable]
	public class WindowRegistration
	{
		[ValueDropdown("WindowTypes")]
		public string WindowType;
		public bool UseAssemblyResolve;

#if UNITY_EDITOR
		public IEnumerable<string> WindowTypes()
		{
			return TypeCache.GetTypesDerivedFrom(typeof(WindowBase)).Select(t => t.FullName);
		}
#endif
	}
	
	[Serializable]
	public class ViewRegistration
	{
		public UiView View;
		[ValueDropdown("ControllerTypes")]
		public string ControllerType;
		public bool UseAssemblyResolve;
		
#if UNITY_EDITOR
		public IEnumerable<string> ControllerTypes()
		{
			var type = View.GetType();
			var uiControllerType = typeof(UiController<>);

			return TypeCache.GetTypesDerivedFrom(uiControllerType).Where(t  => t.BaseType!.GenericTypeArguments.Contains(type)).Select(t => t.FullName);
		}
#endif
	}
}