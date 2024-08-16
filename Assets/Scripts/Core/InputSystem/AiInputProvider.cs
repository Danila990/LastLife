using Core.Entity.Characters.Adapters;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using UnityEngine;

namespace Core.InputSystem
{
	public class AiInputProvider : MonoBehaviour
	{
		[SerializeField] private BehaviourTreeOwner _behaviourTreeOwner;
		[SerializeField] private Blackboard _blackboard;
		[SerializeField] private AiCharacterAdapter _characterAdapter;
		public AiCharacterAdapter AiCharacterAdapter => _characterAdapter;
		
		public void Setup(
			Blackboard blackboard, 
			BehaviourTreeOwner behaviourTreeOwner,
			AiCharacterAdapter characterAdapter
		)
		{
			_characterAdapter = characterAdapter;
			_blackboard = blackboard;
			_behaviourTreeOwner = behaviourTreeOwner;
		}
	}
}