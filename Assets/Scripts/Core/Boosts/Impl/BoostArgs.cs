using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Boosts.Impl
{
	[Serializable]
	public struct BoostArgs
	{
		[ValueDropdown("@BoostTypes.GetTypes()")]
		[GUIColor("GetColor")]
		public string Type;
		public BoostCategory Category;
		public float Duration;
		public float Value;
		
#if UNITY_EDITOR
		private Color GetColor()
		{
			return BoostTypes.GetColorByType(Type);
		}
#endif

		public override string ToString()
		{
			return $"Boost:{Type} | Category {Category} | Duration {Duration} | Value {Value}";
		}
	}

	[Serializable]
	public enum BoostCategory
	{
		Valued,
		Special
	}
}
