using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Core.Actions.SpecialAbilities
{
	[Serializable]
	public struct AbilityPairs
	{
		[TableList] public AbilityPair[] AbilityPair;
	}
	
	[Serializable]
	public struct AbilityPair : IEqualityComparer<AbilityPair>, IEquatable<AbilityPair>
	{
		[ValueDropdown("@ObjectsData.GetCharacters()")]
		public string CharacterId;
		[ValueDropdown("@ObjectsData.GetAbilities()")]
		public string AbilityId;

		public bool Equals(AbilityPair x, AbilityPair y)
		{
			return x.CharacterId == y.CharacterId;
		}
		
		public bool Equals(AbilityPair other)
		{
			return CharacterId == other.CharacterId;
		}
		
		public override int GetHashCode()
		{
			return CharacterId != null ? CharacterId.GetHashCode() : 0;
		}

		public int GetHashCode(AbilityPair obj)
		{
			return obj.CharacterId != null ? obj.CharacterId.GetHashCode() : 0;
		}
	}
}