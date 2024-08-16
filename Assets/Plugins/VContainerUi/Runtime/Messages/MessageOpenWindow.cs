using System;
using VContainerUi.Interfaces;

namespace VContainerUi.Messages
{
	public readonly struct MessageOpenWindow : IUiMessage
	{
		public readonly Type Type;
		public readonly string Name;
		public readonly bool CloseLastPopUp;
		public UiScope UiScope { get; }

		public MessageOpenWindow(Type type, UiScope scope = UiScope.Local)
		{
			Type = type;
			Name = null;
			UiScope = scope;
			CloseLastPopUp = false;
		}

		public MessageOpenWindow(string name, bool closeLastPopUp = false, UiScope scope = UiScope.Local)
		{
			Type = null;
			Name = name;
			CloseLastPopUp = closeLastPopUp;
			UiScope = scope;
		}
	}
}