using UnityEngine;

namespace Analytic.GameAnalytic
{
    [CreateAssetMenu(menuName = "Settings/ApplicationQuitSettings", fileName = "ApplicationQuitSettings")]
    public class ApplicationQuitSettings : ScriptableObject, IApplicationQuitSettings
    {
        public ApplicationQuitBridge BridgePrefab;
        public ApplicationQuitBridge Bridge => BridgePrefab;
    }

    public interface IApplicationQuitSettings
    {
        public ApplicationQuitBridge Bridge { get; }
    }
}