using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.SignalRecieverCommon
{
	public abstract class MonoAnimationTask : MonoBehaviour
	{
		public abstract UniTask AsyncAnimation(CancellationToken token = default);
	}
}