using System;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RemoteServer
{
    public class RemoteAccess : IRemoteAccess
    {
        private readonly IRemoteServerRequster _remoteServerRequester;
        private readonly string _uid;
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        public RemoteAccess(IRemoteServerRequster remoteServerRequester)
        {
            _remoteServerRequester = remoteServerRequester;
            _uid = CreateMD5(SystemInfo.deviceUniqueIdentifier);
        }

        public async UniTask<bool> CheckRemoteAccess()
        {
            return await _remoteServerRequester.RequestToServer($"api/v1/auth/{GetHashedUid(_uid)}");
        }

        public async UniTask<bool> RegisterRemoteAccess(string key)
        {
            return await _remoteServerRequester.RequestToServer($"api/v1/reg/{GetHashedUid(key)}/{PackBase64(_uid)}");
        }

        private string GetHashedUid(string value)
        {
            var hash = CreateMD5(value + GetCurrMinutes());
            hash = PackBase64(hash);
            return hash;
        }

        private static string PackBase64(string base64)
        {
            base64 = base64.Replace("/", "%2F")
                .Replace("+", "%2B");
            return base64;
        }

        private static string CreatePackedMD5(string input)
        {
            return PackBase64(CreateMD5(input));
        }
        
        private long GetCurrMinutes()
        {
            //var now = DateTime.UtcNow;
            //var elapsedTime = now.Subtract(EpochStart);
            return 1;
            //return (long)elapsedTime.TotalMinutes;
        }

        private static string CreateMD5(string input)
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}

