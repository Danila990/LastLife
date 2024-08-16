using UnityEngine;
using UnityEngine.Purchasing;
//using Utils;

namespace Analytic
{
    public class MockBusinessAnalyticAdapter : IBusinessAnalyticAdapter
    {
        public void Send(string args)
        {
        }
        public void Send(string args, float value)
        {
// #if UNITY_EDITOR
//             Debug.Log($"Event {args} | value {value}");
// #endif
        }

        public void SendProgression(ProgressionEventStatus status, string progression01)
        {
        }

        public void SendProgression(ProgressionEventStatus status, string progression01, string progression02)
        {
        }

        public void SendBusinessEvent(PurchaseEventArgs args)
        {
            //TODO: SHARE.SetColor()
            Debug.Log($"Mock Bought: {args.purchasedProduct.definition.id}");
        }
    }
}