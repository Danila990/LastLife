using System;
using VContainerUi.Interfaces;

namespace VContainerUi.Messages
{
	public readonly struct MessageCloseWindow: IUiMessage
	{
		public readonly Type Type;
		public readonly IWindow Window;

		public MessageCloseWindow(IWindow window)
		{
			Window = window;
			Type = null;
			UiScope = UiScope.Local;
		}
		
		public MessageCloseWindow(Type type)
		{
			Type = type;
			Window = null;
			UiScope = UiScope.Local;
		}

		public UiScope UiScope { get; }
	}
}