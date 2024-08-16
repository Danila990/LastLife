using System.Collections.Generic;
using UnityEngine;

namespace Core.Boosts
{
	public static class BoostTypes
	{
		public const string SPEED_UP = "SpeedUp";
		public const string JUMP_UP = "JumpUp";
		public const string DAMAGE = "Damage";
		public const string HP = "HP";
		
		private static IEnumerable<string> GetTypes()
		{
			yield return SPEED_UP;
			yield return JUMP_UP;
			yield return DAMAGE;
			yield return HP;
		}

		public static Color GetColorByType(string type)
		{
			return type switch
			{
				SPEED_UP => Color.yellow,
				JUMP_UP => Parse("#FF9100"),
				DAMAGE => Color.red,
				HP => Color.green,
				_ => Color.white
			};
		}

		private static Color Parse(string strColor) => ColorUtility.TryParseHtmlString(strColor, out var color) ? color : Color.cyan;
	}
}
