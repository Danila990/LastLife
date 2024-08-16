using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Tests.UITest.Utils
{
	public static class DiUtils
	{
		public static LifetimeScope SceneScope
		{
			get
			{
				return LifetimeScope.Find<LifetimeScope>(SceneManager.GetActiveScene());
			}
		}
	}
}