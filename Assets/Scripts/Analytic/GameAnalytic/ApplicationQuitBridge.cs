using System;
using UnityEngine;

namespace Analytic.GameAnalytic
{
    public class ApplicationQuitBridge : MonoBehaviour
    {
        public event Action OnAppQuit;
        private void OnApplicationQuit()
        {
            OnAppQuit?.Invoke();
        }
    }
}