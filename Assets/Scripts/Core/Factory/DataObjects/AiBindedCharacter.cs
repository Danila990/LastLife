using System;
using System.Collections.Generic;
using Core.Entity.Characters.Adapters;
using Sirenix.OdinInspector;

namespace Core.Factory.DataObjects
{
    [Serializable]
    public class AiBindedCharacter
    {
        public AiCharacterAdapter AiAdapter;
#if UNITY_EDITOR
        [ValueDropdown(nameof(Ids))]
#endif
        public string CharacterId;
#if UNITY_EDITOR
        public IEnumerable<string> AvailableIds;
        public IEnumerable<string> Ids()
        {
            return AvailableIds;
        }
#endif
    }
    
    [Serializable]
    public class AiBindedHead
    {
	    public AiHeadAdapter AiAdapter;
	    
#if UNITY_EDITOR
	    [ValueDropdown(nameof(Ids))]
#endif
	    
	    public string CharacterId;
#if UNITY_EDITOR
	    public IEnumerable<string> AvailableIds;
	    public IEnumerable<string> Ids()
	    {
		    return AvailableIds;
	    }
#endif
    }
}