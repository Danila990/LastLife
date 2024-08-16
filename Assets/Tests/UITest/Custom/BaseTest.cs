#if UNITY_EDITOR

using System.Threading;
using NUnit.Framework;
using VContainer.Extensions;
using VContainer.Unity;

namespace Tests.UITest.Custom
{
	public abstract class BaseTest
	{
		private CancellationTokenSource _cancellationTokenSource;

		public CancellationToken CancellationToken => _cancellationTokenSource.Token;
		
		[SetUp]
		public virtual void SetUp()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			OverrideRegistration();
			if (VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance().Container == null)
			{
				VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance().Build();
			}
		}
		
		protected virtual void OverrideRegistration() { }
		
		[TearDown]
		public virtual void TearDown()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			InstallerSubstitution.Clear();
		
		}
	}
}
#endif