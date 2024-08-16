using System.Linq;
using UnityEngine;
using VContainer;

namespace Core.Entity.Ai.AiItem
{
	public class MonoItemProvider : MonoBehaviour
	{
		public MonoAiItem[] MonoItems;
		
		public MonoAiItem GetItemById(string id)
			=> MonoItems.FirstOrDefault(item => item.ItemId == id);


		public void Created(IObjectResolver resolver)
		{
			foreach (var monoAiItem in MonoItems)
			{
				resolver.Inject(monoAiItem);
			}
		}
	}
}