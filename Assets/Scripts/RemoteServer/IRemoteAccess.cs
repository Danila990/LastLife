using Cysharp.Threading.Tasks;

namespace RemoteServer
{
    public interface IRemoteAccess
    {
        public UniTask<bool> CheckRemoteAccess();
        public UniTask<bool> RegisterRemoteAccess(string key);
    }
}