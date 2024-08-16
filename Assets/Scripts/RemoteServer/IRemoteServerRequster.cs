using Cysharp.Threading.Tasks;

namespace RemoteServer
{
    public interface IRemoteServerRequster
    {
        public UniTask<bool> RequestToServer(string endpoint);
    }
}