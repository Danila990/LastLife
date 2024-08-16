using System;
using System.Collections.Generic;
using System.Linq;
using Core.Actions.SpecialAbilities;
using UnityEngine;
using Utils;

namespace Core.Actions
{
	[CreateAssetMenu(menuName = SoNames.FACTORY_DATA + nameof(ScriptableActionsData), fileName = nameof(ScriptableActionsData))]
	public class ScriptableActionsData : ScriptableObject, IScriptableActionsData
	{
		[SerializeField] private AbstractScriptableEntityAction[] _abstractScriptableEntityActions;

		public IReadOnlyList<AbstractScriptableEntityAction> Actions => _abstractScriptableEntityActions;
		[field:SerializeField] public AbilityPairs AbilityPairs { get; set; }

#if UNITY_EDITOR
		/// <summary>
		/// Editor Only
		/// </summary>
		public static ScriptableActionsData EditorInstance;

		public void AddActionEditor(AbstractScriptableEntityAction abstractScriptableEntityAction)
		{
			Array.Resize(ref _abstractScriptableEntityActions, _abstractScriptableEntityActions.Length + 1);
			_abstractScriptableEntityActions[^1] = abstractScriptableEntityAction;
			
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
		}

		public bool Contains(AbstractScriptableEntityAction abstractScriptableEntityAction)
		{
			return _abstractScriptableEntityActions.Contains(abstractScriptableEntityAction);
		}
		
		private void OnEnable()
		{
			EditorInstance = this;
		}
#endif
	}

	public interface IScriptableActionsData
	{
		IReadOnlyList<AbstractScriptableEntityAction> Actions { get; }
		AbilityPairs AbilityPairs { get; }
	} 
}