using System.Collections.Generic;
using Core.HealthSystem;
using UniRx;

namespace Utils
{
	public static class Extensions
	{
		public static bool InBounds<T>(this int index, T[] array)
			=> index >= 0 && index < array.Length;
		
		public static bool InBounds<T>(this int index, List<T> list)
			=> index >= 0 && index < list.Count;
		public static bool InBounds<T>(this int index, ICollection<T> list)
			=> index >= 0 && index < list.Count;
		public static bool InBounds<T>(this int index, IReactiveCollection<T> list)
			=> index >= 0 && index < list.Count;
		
		public static string GetDiedMeta(this DiedArgs args)
		{
			if (args.MetaDamage is not null)
			{
				return args.MetaDamage.SourceId;
			}
			
			if (args.DiedFrom is not null)
			{
				return args.DiedFrom.SourceId;
			}
			return "unknown";
		}
	}
}
