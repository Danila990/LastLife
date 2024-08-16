#if RELEASE_BRANCH
using System;
using System.Collections.Generic;
using GameAnalyticsSDK;
using MessagePipe;
using UnityEngine;
using UnityEngine.Purchasing;
using VContainer.Unity;

namespace Analytic.GameAnalytic
{
    public struct ApplicationQuit { }

    public class GameAnalyticAdapter : IProgressionAnalyticAdapter, IGameAnalyticsATTListener, IAdvertisingAnalyticAdapter, IInitializable, IResourceAnalyticAdapter,IBusinessAnalyticAdapter
    {
        private readonly ISubscriber<ApplicationQuit> _subscriber;
        private readonly Dictionary<ProgressionEventStatus, GAProgressionStatus> _convert = new();
        private const string FAKE_RECEIPT_DATA = "ThisIsFakeReceiptData";
        private string _lastEventSended;
        
        public GameAnalyticAdapter(IApplicationQuitObserver observer)
        {
            observer.OnAppQuit += OnAppQuit;
        }

        private void OnAppQuit()
        {
            GameAnalytics.NewDesignEvent($"LastEvent:{_lastEventSended}");
        }
        
        public void Initialize()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                GameAnalytics.RequestTrackingAuthorization(this);
            }
            else
            {
                GameAnalytics.Initialize();
            }
            _convert.Add(ProgressionEventStatus.Undefined, GAProgressionStatus.Undefined);
            _convert.Add(ProgressionEventStatus.Start, GAProgressionStatus.Start);
            _convert.Add(ProgressionEventStatus.Complete, GAProgressionStatus.Complete);
            _convert.Add(ProgressionEventStatus.Fail, GAProgressionStatus.Fail);
            
        }

        public void Send(string args)
        {
            GameAnalytics.NewDesignEvent(args);
            _lastEventSended = args;
        }
        
        public void Send(string args, float value)
        {
            GameAnalytics.NewDesignEvent(args, value);
            _lastEventSended = args;
        }

        public void SendResourceEvent(ResourceEventType eventType, string currency, float amount, string itemType, string itemId)
        {
            GameAnalytics.NewResourceEvent(GetGaResourceFlowType(eventType), currency, amount, itemType, itemId);
        }

        public void SendADEvent(in ADEventArgs args)
        {
            var gaAdType = ProxyAdType(args.ADType);
            var gaError = ProxyAdError(args.Error);
            var gaEventType = ProxyAdEventType(args.Type);

            GameAnalytics.NewAdEvent(
                gaEventType, 
                gaAdType, 
                args.SkadNetworkId,
                string.IsNullOrWhiteSpace(args.Placement) ? "default" : args.Placement, 
                gaError);
        }
        
        public void SendBusinessEvent(PurchaseEventArgs args)
        {
            var amount = decimal.ToInt32(args.purchasedProduct.metadata.localizedPrice * 100);
            var iso = args.purchasedProduct.metadata.isoCurrencyCode;
            var reciept = args.purchasedProduct.receipt;

            try
            {
#if UNITY_ANDROID
                Receipt receiptAndroid = JsonUtility.FromJson<Receipt>(reciept);
                PayloadAndroid receiptPayload = JsonUtility.FromJson<PayloadAndroid>(receiptAndroid.Payload);
                GameAnalytics.NewBusinessEventGooglePlay(iso,amount,"IAP",args.purchasedProduct.definition.id,"IAP",receiptPayload.json,receiptPayload.signature);
#endif
#if UNITY_IOS
                GameAnalytics.NewBusinessEventIOSAutoFetchReceipt(iso,amount,"IAP",args.purchasedProduct.definition.id,"iap");
                GameAnalytics.NewBusinessEventIOS(iso,amount,"IAP",args.purchasedProduct.definition.id,"iap",reciept);
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
        
        private static GAResourceFlowType GetGaResourceFlowType(ResourceEventType type)
        {
            return type switch
            {
                ResourceEventType.Add => GAResourceFlowType.Source,
                ResourceEventType.Remove => GAResourceFlowType.Sink,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static GAAdAction ProxyAdEventType(AdEventType type)
        {
            return type switch
            {
                AdEventType.Undefined => GAAdAction.Undefined,
                AdEventType.Clicked => GAAdAction.Clicked,
                AdEventType.Show => GAAdAction.Show,
                AdEventType.FailedShow => GAAdAction.FailedShow,
                AdEventType.RewardReceived => GAAdAction.RewardReceived,
                AdEventType.Request => GAAdAction.Request,
                AdEventType.Loaded => GAAdAction.Loaded,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        private static GAAdType ProxyAdType(AdType type)
        {
            return type switch
            {
                AdType.Undefined => GAAdType.Undefined,
                AdType.Video => GAAdType.Video,
                AdType.RewardedVideo => GAAdType.RewardedVideo,
                AdType.Playable => GAAdType.Playable,
                AdType.Interstitial => GAAdType.Interstitial,
                AdType.OfferWall => GAAdType.OfferWall,
                AdType.Banner => GAAdType.Banner,
                AdType.AppOpen => GAAdType.AppOpen,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        private static GAAdError ProxyAdError(AdError error)
        {
            return error switch
            {
                AdError.Undefined => GAAdError.Undefined,
                AdError.Unknown => GAAdError.Unknown,
                AdError.Offline => GAAdError.Offline,
                AdError.NoFill => GAAdError.NoFill,
                AdError.InternalError => GAAdError.InternalError,
                AdError.InvalidRequest => GAAdError.InvalidRequest,
                AdError.UnableToPrecache => GAAdError.UnableToPrecache,
                _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
            };
        }

        public void SendProgression(ProgressionEventStatus status, string progression01)
        {
            var gaStatus = _convert[status];
            GameAnalytics.NewProgressionEvent(gaStatus,progression01);
        }

        public void SendProgression(ProgressionEventStatus status, string progression01, string progression02)
        {
            var gaStatus = _convert[status];
            GameAnalytics.NewProgressionEvent(gaStatus,progression01,progression02);
        }

        public void GameAnalyticsATTListenerNotDetermined()
        {
            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerRestricted()
        {
            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerDenied()
        {
            GameAnalytics.Initialize();
        }

        public void GameAnalyticsATTListenerAuthorized()
        {
            GameAnalytics.Initialize();
        }
    }
}
#endif


