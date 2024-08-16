using UnityEngine;

namespace Utils
{
	public static class GameObjectUtils
	{
		public static GameObject FindObjectByName(GameObject parent, string targetName)
		{
			foreach (Transform child in parent.transform)
			{
				if(child.gameObject.name == "PuppetMaster")
					continue;
				
				if(child.gameObject.name == targetName)
				{
					return child.gameObject;
				}
				GameObject result = FindObjectByName(child.gameObject, targetName);
				if (result != null)
				{
					return result;
				}
			}
			
			return null;
		}
	}
}
