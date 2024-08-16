using System;
using System.Collections.Generic;
using System.Linq;
using Core.Inventory.Items.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Constants;

namespace Core.Entity.EntityUpgrade.Upgrades.Impl
{
	[Serializable]
	[HideReferenceObjectPicker]
	public class WeaponParameterUpgradeParameters : Abs.EntityUpgradeParameters
	{
		[SerializeField, ValueDropdown("GetNames")] private string _prefKey;
		[SerializeField] private float[] _values;
		public override string PrefKey => _prefKey;
		
		public override void ApplyUpgrade(ref UpgradeableParameters parameters, int level)
		{
			if(level == 0)
				return;
			
			var value = parameters.GetValue(_prefKey);
			parameters.SetValue(_prefKey, GetValue(level - 1) + value);
		}

		private float GetValue(int level)
		{
#if UNITY_EDITOR
			if (_values is null || _values.Length == 0)
			{
				Debug.LogError($"Weapon parameter upgrade parameters {_prefKey} has no values");
				return 0f;
			}
#endif
			return _values[Mathf.Clamp(level, 0, _values.Length - 1)];
		}

#if UNITY_EDITOR
		[Title("Debug")]
		[ShowInInspector] private float _initialValue;
		[ShowInInspector] private float[] _testValues => _values?.Select(x => x + _initialValue).ToArray();
		
		public IEnumerable<string> GetNames()
		{
			var list = new List<string>();
			foreach (var upgrade in typeof(WeaponConsts).GetFields())
			{
				var value = upgrade.GetValue(null) as string;
				if (!string.IsNullOrEmpty(value))
				{
					list.Add(value);
				}
			}
			return list;
		}
#endif
	}
}