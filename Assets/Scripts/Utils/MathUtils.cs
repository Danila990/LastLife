using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public static class MathUtils
	{
		public static void GetPointsAroundOriginAsArray(Vector3 origin, ref Vector3[] buffer, float radius = 2f)
		{
			for (var i = 0; i < buffer.Length; i++)
			{
				var angle = i * 360f / buffer.Length;
				var direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
				buffer[i] = origin + direction * radius;
			}
		}
		
		public static List<Vector3> GetPointsAroundOriginAsList(Vector3 origin, int bufferSize = 20, float radius = 2f)
		{
			var buffer = new List<Vector3>(bufferSize);
				
			for (int i = 0; i < bufferSize; i++)
			{
				float angle = i * 360f / bufferSize;
				Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
				buffer.Add(origin + direction * radius);
			}

			return buffer;
		}
		
		public static int CountDigits(this int number)
		{
			number = Mathf.Abs(number);
			if (number == 0)
			{
				return 1;
			}

			int count = 0;
			while (number != 0)
			{
				count++;
				number /= 10;
			}

			return count;
		}
	}
}
