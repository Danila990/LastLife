using UnityEngine;
using VContainer;
using VContainer.Extensions;

namespace Core.Services.SaveSystem
{
	public class SaveSystemInstaller : MonoInstaller
	{
		[SerializeField] private string _saveSystemFileName;
		
		public override void Install(IContainerBuilder builder)
		{
			builder.Register<SaveSystemService>(Lifetime.Singleton)
				.AsImplementedInterfaces()
				.WithParameter(_saveSystemFileName);
		}
	}
}