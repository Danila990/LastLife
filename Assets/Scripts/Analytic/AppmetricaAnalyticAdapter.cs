#if false //TODO APPMETRICA
using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class AppmetricaAnalyticAdapter : IAppmetricaAnalyticAdapter
{
    public void Send(string args)
    {
    }

    public void SendProgression(ProgressionEventStatus status, string progression01)
    {
    }

    public void SendProgression(ProgressionEventStatus status, string progression01, string progression02)
    {
    }

    public void SendBusinessEvent(PurchaseEventArgs args)
    {
        var product = args.purchasedProduct;
        var currency = product.metadata.isoCurrencyCode;
        var price = product.metadata.localizedPrice;
        var revenue = new YandexAppMetricaRevenue(price, currency);
        if(product.receipt != null) {
            var yaReceipt = new YandexAppMetricaReceipt();
            var receipt = JsonUtility.FromJson<Receipt>(product.receipt);
            if(receipt.Payload.Equals("ThisIsFakeReceiptData"))
            {
                Debug.Log($"AppmetricaFakeData {revenue}");
                AppMetrica.Instance.ReportRevenue(revenue);
                return;
            }
#if UNITY_ANDROID
            var payloadAndroid = JsonUtility.FromJson<PayloadAndroid>(receipt.Payload);
            yaReceipt.Signature = payloadAndroid.Signature;
            yaReceipt.Data = payloadAndroid.Json;
#elif UNITY_IPHONE
            yaReceipt.TransactionID = receipt.TransactionID;
            yaReceipt.Data = receipt.Payload;
#endif
            revenue.Receipt = yaReceipt;
        }
        AppMetrica.Instance.ReportRevenue(revenue);
    }
}
#endif