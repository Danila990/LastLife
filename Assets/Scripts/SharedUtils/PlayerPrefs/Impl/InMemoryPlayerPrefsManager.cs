using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SharedUtils.PlayerPrefs.Impl
{
    public class InMemoryPlayerPrefsManager : IPlayerPrefsManager
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        
        public bool HasKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public void DeleteKey(string key)
        {
            _values.Remove(key);
        }

        public void DeleteAll()
        {
            _values.Clear();
        }

        public void Save()
        {
            var str = string.Join(";", _values.Select(x => x.Key + "=" + x.Value).ToArray());
            Debug.Log("Save player prefs \n" + str);
        }

        public void SetValue<T>(string key, T value)
        {

            _values[key] = value;
        }
        
        public T GetValue<T>(string key)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return (T) value;
            }
            return default(T);
        }
        
        public T GetValue<T>(string key, T defaultValue)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return (T) value;
            }
            return defaultValue;
        }
    }
}