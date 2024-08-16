using System.Net.Http;
using Cysharp.Threading.Tasks;

namespace RemoteServer
{
    public class RemoteServerRequester : IRemoteServerRequster
    {
        private const string SERVER_ADDRESS = "http://193.187.175.108:1337/";
        private readonly HttpClient _httpClient = new HttpClient();

        public async UniTask<bool> RequestToServer(string endpoint)
        {
            var request = GetAsync(SERVER_ADDRESS+endpoint);
            var result = await UniTask.WhenAny(request, UniTask.Delay(1000));
            if (result.hasResultLeft)
            {
                return result.result == "approve";
            }
            return false;
        }
        
        private async UniTask<string> GetAsync(string uri)
        {
            using var response = await _httpClient.GetAsync(uri);
            return await response.Content.ReadAsStringAsync();
        }
    }
}