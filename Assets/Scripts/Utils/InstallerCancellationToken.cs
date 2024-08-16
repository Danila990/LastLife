using System.Threading;

namespace Utils
{
	public class InstallerCancellationToken
	{
		public CancellationToken Token;

		public InstallerCancellationToken(CancellationToken cancellationToken)
		{
			Token = cancellationToken;
		}
	}
	
	public class ProjectCancellationToken
	{
		public CancellationToken Token;

		public ProjectCancellationToken(CancellationToken cancellationToken)
		{
			Token = cancellationToken;
		}
	}
}