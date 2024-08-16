using System;
using System.Collections.Generic;

namespace Core.Inventory.Items.Weapon
{
	[Serializable]
	public struct UpgradeableParameters
	{
		public UpgradeableParameters(IDictionary<string, float> parameters)
		{
			Parameters = parameters;
		}

		private IDictionary<string, float> Parameters { get; }
		
		public float GetValue(string key)
		{
			return Parameters[key];
		}
		
		public void SetValue(string key, float value)
		{
			Parameters[key] = value;
		}
		
#if UNITY_INCLUDE_TESTS
		public IDictionary<string, float> GetParameters => Parameters;
#endif
	}
}