using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Core.Quests
{
	public class NodeTester : MonoBehaviour
	{
		[Inject] private readonly IQuestService _questService;


		[Button]
		private void CompleteActiveQuests()
		{
			foreach (var node in _questService.ActiveTree.Value.FinalNodes.Values)
			{
				node.SetValue(node.MaxValue);
			}
		}

		[Button]
		private void CompleteQuest()
		{
			var node = _questService.ActiveTree.Value.FinalNodes.Values.FirstOrDefault(x => x.Value < x.MaxValue);
			node?.SetValue(node.MaxValue);
		}
	}
}
