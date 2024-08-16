using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Dialogue.Services.Interfaces
{
	public abstract class MonoModuleDialogueArgs : MonoBehaviour, IModuleArgs
	{
		[field:SerializeField, ValueDropdown("GetModuleIds")] public string ModuleId { get; set; }

#if UNITY_EDITOR
		private IEnumerable<string> GetModuleIds()
		{
			// .Concat(TypeCache.GetTypesDerivedFrom<LifetimeScope>()
			// 		.Select(type => type.FullName))
			// 	.ToArray();
			return TypeCache.GetTypesDerivedFrom<IDialogueModule>().Select(type => type.Name);
		}
#endif
	}
}