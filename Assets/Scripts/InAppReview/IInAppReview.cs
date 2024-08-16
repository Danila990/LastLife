using System.Threading;
using Cysharp.Threading.Tasks;

namespace InAppReview
{
	public interface IInAppReview
	{
		public UniTask RequestInAppReview(CancellationToken token);
	}
}