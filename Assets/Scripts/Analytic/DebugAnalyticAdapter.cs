using SharedUtils;
using UnityEngine;

namespace Analytic
{
	public class DebugAnalyticAdapter : IAnalyticAdapter, IResourceAnalyticAdapter
	{
		public void Send(string args)
		{
#if UNITY_EDITOR
			Debug.Log($"Event {args}");
#endif
		}
		public void Send(string args, float value)
		{
#if UNITY_EDITOR
			Debug.Log($"Event {args} | value {value}");
#endif
		}
		
		public void SendResourceEvent(ResourceEventType eventType, string currency, float amount, string itemType, string itemId)
		{
#if UNITY_EDITOR
			Debug.Log($"Resource Event: {eventType.ToString().SetColor("yellow")} {$"[{currency}|{amount}|{itemType}|{itemId}]".SetColor()}");
#endif
		}

		public void SendProgression(ProgressionEventStatus status, string progression01)
		{
#if UNITY_EDITOR
			Debug.Log($"Progression {status} - {progression01}");
#endif
		}

		public void SendProgression(ProgressionEventStatus status, string progression01, string progression02)
		{
#if UNITY_EDITOR
			Debug.Log($"Progression {status} - {progression01} - {progression02}");
#endif
		}
	}
}