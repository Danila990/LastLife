using System;
using System.Collections;
using System.Collections.Generic;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Utils;

namespace GameSettings
{
	public interface IGameSetting
	{
		string SettingKeyName { get; }
		void Initialize();
		T GetValue<T>(string key, GameSetting.ParameterType parameterType);
		void OverrideValue<T>(string key, GameSetting.ParameterType parameterType, T value);
		bool HasValue(string key, GameSetting.ParameterType parameterType);
		IDictionary<string, T> GetDictionary<T>(GameSetting.ParameterType parameterType);
	}
	
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(GameSetting), fileName = nameof(GameSetting))]
	public class GameSetting : ScriptableObject, IGameSetting
	{
		[field:SerializeField, GUIColor("green")] public string SettingKeyName { get; private set; }
		[field:SerializeField] public string SettingNormalName { get; private set; }
		[SerializeField] protected ParameterData[] Parameters;
		[NonSerialized] protected readonly IDictionary<ParameterType, IDictionary> SettingParameters = new Dictionary<ParameterType, IDictionary>();

		[NonSerialized] protected bool Initialized;
		public static bool ViolenceStatus;

		public override string ToString()
		{
			var str = $"{SettingKeyName.SetColor()}";
			if (Initialized)
			{
				foreach (var parameter in SettingParameters.Values)
				{
					foreach (var value in parameter)
					{
						str +=  $"\n kv: {value}";
					}
				}
			}
			else
			{
				foreach (var parameter in Parameters)
				{
					str +=  $"\n {parameter.Key} : {parameter.GetValue()}";
				}
			}
			
			return str;
		}
		
		public virtual void Initialize()
		{
			if (Parameters == null)
				return;
			SettingParameters.Clear();
			
			foreach (var parameter in Parameters)
			{
				switch (parameter.ParameterType)
				{
					case ParameterType.Float:
						SetValue(parameter.ParameterType, parameter.Key, parameter.FloatValue);
                        break;
					case ParameterType.String:
						SetValue(parameter.ParameterType, parameter.Key, parameter.StringValue);
						break;
					case ParameterType.Bool:
						SetValue(parameter.ParameterType, parameter.Key, parameter.BoolValue);
						break;
					case ParameterType.Int:
						SetValue(parameter.ParameterType, parameter.Key, parameter.IntValue);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(parameter), nameof(GameSetting));
				}
			}

			Initialized = true;
		}

		public IDictionary<string, T> GetDictionary<T>(ParameterType parameterType)
		{
			if (SettingParameters.TryGetValue(parameterType, out var value))
			{
				return (IDictionary<string, T>)value;
			}
			else
			{
				var dict = new Dictionary<string, T>();
				SettingParameters[parameterType] = dict;
				return dict;
			}
		}

		public T GetValue<T>(string key, ParameterType parameterType)
		{
			return GetDictionary<T>(parameterType)[key];
		}
		
		public void OverrideValue<T>(string key, ParameterType parameterType, T value)
		{
			var dictionary = GetDictionary<T>(parameterType);
			dictionary[key] = value;
		}
		
		public bool HasValue(string key, ParameterType parameterType)
		{
			if(SettingParameters.TryGetValue(parameterType, out var value))
			{
				return value.Contains(value);
			}
			return false;
		}

		private void SetValue<T>(ParameterType parameterType, string key, T value)
		{
			var dictionary = GetDictionary<T>(parameterType);
			if (!dictionary.TryAdd(key, value))
			{
				Debug.LogWarning($"Key: {key} value: {value.ToString()} contains in {dictionary}");
			}
		}
		
		public enum ParameterType
		{
			Float,
			Int,
			String,
			Bool
		}
#if UNITY_EDITOR
		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			if (SettingParameters == null || SettingParameters.Count == 0)
				return;
			
			foreach (var upgrade in SettingParameters)
			{
				EditorGUILayout.BeginHorizontal();
				GUI.color = Color.green;
				EditorGUILayout.LabelField(new GUIContent(upgrade.Key.ToString()), GUILayout.MaxWidth(100));
				GUI.color = Color.white;

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginVertical();

				foreach (DictionaryEntry parameter in upgrade.Value)
				{
					EditorGUILayout.BeginVertical();
					EditorGUILayout.LabelField(parameter.Key.ToString());
					EditorGUILayout.LabelField(new GUIContent(parameter.Value.ToString()), GUILayout.MaxWidth(250));
					EditorGUILayout.EndVertical();
					EditorGUILayout.Separator();
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space(20);
			}
		}
#endif
		[Serializable]
		public struct ParameterData
		{
			public string Key => _key;
			public ParameterType ParameterType => _parameterType;

			[SerializeField] private string _key;
			[SerializeField] private ParameterType _parameterType;
			[ShowIf("_parameterType", ParameterType.String)] public string StringValue;
			[ShowIf("_parameterType", ParameterType.Bool)] public bool BoolValue;
			[ShowIf("_parameterType", ParameterType.Float)] public float FloatValue;
			[ShowIf("_parameterType", ParameterType.Int)] public int IntValue;

			public object GetValue() => ParameterType switch
			{
				ParameterType.Float => FloatValue,
				ParameterType.Int => IntValue,
				ParameterType.String => StringValue,
				ParameterType.Bool => BoolValue,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
	
}