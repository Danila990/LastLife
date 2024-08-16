using System;
using UnityEngine;
using VContainer.Unity;

namespace VContainer.Extensions
{
	public class ScriptableObjectInstaller : ScriptableObject, IInstaller
	{
		public virtual void Install(IContainerBuilder builder)
		{
			throw new NotImplementedException();
		}
	}
}