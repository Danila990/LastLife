using UnityEngine.Purchasing;

namespace Analytic
{
	public interface IAnalyticService
	{
		void SendEvent(string args);
		void SendEvent(string args, float value);
		void SendResourceEvent(ResourceEventType eventType, string currency, float amount, string itemType, string itemId);
		void SendADEvent(in ADEventArgs args);
		void SendProgression(ProgressionEventStatus status, string progression01);
		void SendProgression(ProgressionEventStatus status, string progression01, string progression02);
		void SendBusinessEvent(PurchaseEventArgs args);
	}
}
