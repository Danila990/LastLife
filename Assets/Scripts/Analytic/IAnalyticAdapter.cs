
using UnityEngine.Purchasing;

namespace Analytic
{
    public interface IAnalyticAdapter
    {
        void Send(string args);
        void Send(string args, float value);
    }

    public interface IProgressionAnalyticAdapter : IAnalyticAdapter
    {
        void SendProgression(ProgressionEventStatus status, string progression01);
        void SendProgression(ProgressionEventStatus status, string progression01, string progression02);
    }

    public interface IAdvertisingAnalyticAdapter : IAnalyticAdapter
    {
        void SendADEvent(in ADEventArgs args);
    }

    public interface IBusinessAnalyticAdapter : IAnalyticAdapter
    {
        void SendBusinessEvent(PurchaseEventArgs args);
    }

    public struct ADEventArgs
    {
        public readonly string SkadNetworkId;
        public string Placement;
        public readonly AdEventType Type;
        public readonly AdType ADType;
        public readonly AdError Error;
        public double ECpm;

        public ADEventArgs(string skadNetworkId, string placement, AdEventType type, AdType adType, AdError error)
        {
            SkadNetworkId = skadNetworkId;
            Placement = placement;
            Type = AdEventType.Undefined;
            ADType = AdType.Undefined;
            Error = AdError.Undefined;
            Type = type;
            ADType = adType;
            Error = error;
            ECpm = 0;
        }
    }

    public interface IResourceAnalyticAdapter : IAnalyticAdapter
    {
        /// <summary>
        /// Send Resource Event
        /// </summary>
        /// <param name="eventType">Add (Add) or subtract (Remove) resource.</param>
        /// <param name="currency">Gems, Gold, Etc</param>
        /// <param name="amount">Amount spend or add count.</param>
        /// <param name="itemType">Item Type source from you get it</param>
        /// <param name="itemId">Concrete Item Id</param>
        /// <example>SendResourceEvent(ResourceEventType.Add, "Ticket", 2, "Looting", "BossDrop");</example>
        void SendResourceEvent(ResourceEventType eventType, string currency, float amount, string itemType, string itemId);
    }
    
    public enum ResourceEventType
    {
        Add = 0,
        Remove = 1,
    }
    
    public enum AdType
    {
        Undefined = 0,
        Video = 1,
        RewardedVideo = 2,
        Playable = 3,
        Interstitial = 4,
        OfferWall = 5,
        Banner = 6,
        AppOpen = 7
    }
    
    public enum AdError
    {
        Undefined = 0,
        Unknown = 1,
        Offline = 2,
        NoFill = 3,
        InternalError = 4,
        InvalidRequest = 5,
        UnableToPrecache = 6
    }
    
    public enum AdEventType
    {
        Undefined = 0,
        Clicked = 1,
        Show = 2,
        FailedShow = 3,
        RewardReceived = 4,
        Request = 5,
        Loaded = 6
    }
}