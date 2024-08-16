namespace Analytic
{
    [System.Serializable]
    public class Receipt
    {
        public string Store;
        public string TransactionID;
        public string Payload;
        
        public Receipt()
        {
            Store = TransactionID = Payload = "";
        }
        
        public Receipt(string store, string transactionID, string payload)
        {
            Store = store;
            TransactionID = transactionID;
            Payload = payload;
        }
    }
}