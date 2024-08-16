using MessagePipe;
using UnityEngine;
using VContainer;
using VContainerUi.Abstraction;
using VContainerUi.Interfaces;
using VContainerUi.Messages;

namespace VContainerUi
{
	public static class RegisterExtensions
	{
		public static void RegisterUiView<TController, TView>(this IContainerBuilder builder, TView viewPrefab, Transform parent)
			where TController : UiController<TView>
			where TView : MonoBehaviour, IUiView
		{
			builder.Register<TController>(Lifetime.Singleton)
				.AsImplementedInterfaces().AsSelf();
			
			builder.Register((resolver) =>
				{
					var view = Object.Instantiate(viewPrefab, parent);
					resolver.Inject(view);
					view.gameObject.SetActive(false);
					return view;
				}, Lifetime.Singleton)
				.AsImplementedInterfaces().AsSelf();
		}
		
		public static void RegisterUiViewUnderRegisteredCanvas<TController, TView>(this IContainerBuilder builder, TView viewPrefab)
			where TController : UiController<TView>
			where TView : MonoBehaviour, IUiView
		{
			builder.Register<TController>(Lifetime.Singleton)
				.AsImplementedInterfaces().AsSelf();
			
			builder.Register((resolver) =>
				{
					var canvas = resolver.Resolve<Canvas>();
					var view = Object.Instantiate(viewPrefab, canvas.transform);
					resolver.Inject(view);
					view.gameObject.SetActive(false);
					return view;
				}, Lifetime.Singleton)
				.AsImplementedInterfaces().AsSelf();
		}
		
		public static void RegisterUiSignals(this IContainerBuilder builder, MessagePipeOptions options)
		{
			builder.RegisterMessageBroker<MessageOpenWindow>(options);
			builder.RegisterMessageBroker<MessageCloseWindow>(options);
			builder.RegisterMessageBroker<MessageOpenRootWindow>(options);
			builder.RegisterMessageBroker<MessageShowWindow>(options);
			builder.RegisterMessageBroker<MessageActiveWindow>(options);
			builder.RegisterMessageBroker<MessageFocusWindow>(options);
			builder.RegisterMessageBroker<MessageBackWindow>(options);
		}
	}
}