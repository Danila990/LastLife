using System.Threading;
using Cysharp.Threading.Tasks;
using SharedUtils;

namespace Common.SignalRecieverCommon
{
	public class DelayAnimationTask : MonoAnimationTask
	{
		public float Time;
		
		public async override UniTask AsyncAnimation(CancellationToken token = default)
		{
			await UniTask.Delay(Time.ToSec(), cancellationToken: token);
		}
	}
}