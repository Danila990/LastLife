using LitMotion;
using UnityEngine;

namespace Utils
{
	public static class LMotionUtil
	{
		public static void IsActiveCancel(this MotionHandle handle)
		{
			if (handle.IsActive()) 
			{
				handle.Cancel();
			}
		}
		
		public static void IsActiveComplete(this MotionHandle handle)
		{
			if (handle.IsActive()) 
			{
				handle.Complete();
			}
		}
		
		public static void IsActiveCompleteCancel(this MotionHandle handle)
		{
			if (handle.IsActive()) 
			{
				handle.Complete();
				handle.Cancel();
			}
		}
		
		public static void IsActiveCompleteCancel(this CompositeMotionHandle handle)
		{
			handle?.Complete();
			handle?.Cancel();
		}
		public static MotionBuilder<Vector2Int, NoOptions, Vector2IntMotionAdapter> Create(Vector2Int from, Vector2Int to, float duration)
		{
			return LMotion.Create<Vector2Int, NoOptions, Vector2IntMotionAdapter>(from, to, duration);
		}

		public readonly struct Vector2IntMotionAdapter : IMotionAdapter<Vector2Int, NoOptions>
		{
			public Vector2Int Evaluate(ref Vector2Int startValue, ref Vector2Int endValue, ref NoOptions options, in MotionEvaluationContext context)
			{
				return new Vector2Int(
					(int)Mathf.LerpUnclamped(startValue.x, endValue.x, context.Progress),
					(int)Mathf.LerpUnclamped(startValue.y, endValue.y, context.Progress)
				);
			}
		}
	}
}