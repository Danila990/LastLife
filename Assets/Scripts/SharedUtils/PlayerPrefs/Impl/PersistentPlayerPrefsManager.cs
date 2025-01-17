﻿using System;
using System.Globalization;

namespace SharedUtils.PlayerPrefs.Impl
{
	public class PersistentPlayerPrefsManager : IPlayerPrefsManager
	{
		public void SetValue<T>(string key, T val)
        {
            if (typeof(T) == typeof(int))
            {
                UnityEngine.PlayerPrefs.SetInt(key, Convert.ToInt32(val));
            } 
            else if (typeof(T) == typeof(bool))
            {
                var boolVal = Convert.ToBoolean(val);
                var storedVal = UnityEngine.PlayerPrefs.GetInt(key, boolVal ? 1 : 0);
                UnityEngine.PlayerPrefs.SetInt(key, storedVal);
            } 
            else if (typeof(T) == typeof(float))
            {
                UnityEngine.PlayerPrefs.SetFloat(key, (float) Convert.ToDouble(val));
            } 
            else if (typeof(T) == typeof(long))
            {
                UnityEngine.PlayerPrefs.SetString(key, val.ToString());
            } 
            else if (typeof(T) == typeof(string))
            {
                UnityEngine.PlayerPrefs.SetString(key, val.ToString());
            }
            else if (typeof(T) == typeof(DateTime))
            {
                UnityEngine.PlayerPrefs.SetString(key, Convert.ToDateTime(val).ToString("o", CultureInfo.InvariantCulture));
            }
            
            else
            {
                throw new ArgumentNullException("Cannot store value with type + " + typeof(T).Name + " to PlayerPrefs");
            }
        }

        public T GetValue<T>(string key)
        {
            if(!HasKey(key))
                throw new ArgumentNullException("PlayerPrefs does not contain key " + key);
            
            if (typeof(T) == typeof(int))
            {
                return (T) (object) UnityEngine.PlayerPrefs.GetInt(key);
            }
            if (typeof(T) == typeof(bool))
            {
                var storedVal = UnityEngine.PlayerPrefs.GetInt(key);
                var boolVal = storedVal == 1;
                return (T)(object)boolVal;
            }
            if (typeof(T) == typeof(float))
            {
                return (T)(object)UnityEngine.PlayerPrefs.GetFloat(key);
            }
            if (typeof(T) == typeof(long))
            {
                var strVal = UnityEngine.PlayerPrefs.GetString(key);
                return (T)(object)Convert.ToInt64(strVal);
            }
            if (typeof(T) == typeof(string))
            {
                return (T)(object)UnityEngine.PlayerPrefs.GetString(key);
            }
            if (typeof(T) == typeof(DateTime))
            {
                var stringValue = UnityEngine.PlayerPrefs.GetString(key);
                return (T)(object)DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }

            throw new ArgumentNullException("Cannot get value with type + " + typeof(T).Name + " by key " + key + " from PlayerPrefs");
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            return !HasKey(key) ? defaultValue : GetValue<T>(key);
        }

        public bool HasKey(string key)
        {
            return UnityEngine.PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            UnityEngine.PlayerPrefs.DeleteKey(key);
        }

        public void DeleteAll()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
        }

        public void Save()
        {
            UnityEngine.PlayerPrefs.Save();
        }
	}
}