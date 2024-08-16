using MessagePipe;
using UnityEngine;
using VContainer;

namespace Installer.Sandbox
{
	public abstract class MessagePipeInstaller : MonoBehaviour
	{
		public abstract void Install(IContainerBuilder builder, MessagePipeOptions options);
	}
}
