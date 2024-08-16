using System;
using UnityEngine;
using VContainer.Unity;

namespace VContainer.Extensions
{
	public class MonoInstaller : MonoBehaviour, IInstaller
	{
		public virtual void Install(IContainerBuilder builder)
		{
			throw new NotImplementedException();
		}
	}
}