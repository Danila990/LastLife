namespace Analytic
{
    [System.Serializable]
    public class PayloadAndroid
    {
        public string Json;
        public string Signature;
        
        public PayloadAndroid()
        {
            Json = Signature = "";
        }

        public PayloadAndroid(string json, string signature)
        {
            Json = json;
            Signature = signature;
        }
    }
}