using System;
using Core.ResourcesSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Db
{
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(ResourceIconData), fileName = "ResourceIconData")]
	public class ResourceIconData : ScriptableObject
	{
		[SerializeField, TableList] private ResourceIcon[] _resourceIcons;
			
		public Sprite GetIcon(ResourceType resourceType)
		{
			foreach (var resource in _resourceIcons)
			{
				if (resource.ResourceType == resourceType)
				{
					return resource.Icon;
				}
			}
			
			return null;
		}
		
		[Serializable]
		private struct ResourceIcon
		{
			public ResourceType ResourceType;
			public Sprite Icon;
		}
	}
}