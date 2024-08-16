using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Extensions;

namespace Installer
{
	[CreateAssetMenu(menuName = SoNames.INSTALLERS, fileName = "GenericScriptableInstaller")]
	public class ScriptableInstallerGenericInterfacesReg : ScriptableObjectInstaller
	{
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data")] private ScriptableObject[] ScriptableObjects;
		
		public override void Install(IContainerBuilder builder)
		{
			foreach (var installer in ScriptableObjects)
			{
				builder.RegisterInstance(installer).AsImplementedInterfaces();
			}
		}
	}
}