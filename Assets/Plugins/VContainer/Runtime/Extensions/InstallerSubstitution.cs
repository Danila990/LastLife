using System;
using System.Collections.Generic;

namespace VContainer.Extensions
{
	public static class InstallerSubstitution
	{
		private static readonly Dictionary<Type, Type> Substitution = new Dictionary<Type, Type>();

		public static void Clear()
		{
			Substitution.Clear();
		}
		
		public static void Substitute<TServiceInterface, TNewServiceImplementation>() 
			where TNewServiceImplementation : TServiceInterface
		{
			Substitution[typeof(TServiceInterface)] = typeof(TNewServiceImplementation);
		}

		public static bool TryGetSubstitution(Type subInterface, out Type substitution)
		{
			return Substitution.TryGetValue(subInterface, out substitution);
		}
	}
}