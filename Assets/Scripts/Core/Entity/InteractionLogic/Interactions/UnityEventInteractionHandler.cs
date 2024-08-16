using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class UnityEventInteractionHandler : MonoBehaviour
	{
		[SerializeField] private GenericInteraction _interaction;
		[SerializeField] private UnityEvent OnUsed;

		private void Awake()
		{
			_interaction.Used.Subscribe(_ => OnInteract()).AddTo(_interaction);
		}

		private void OnInteract()
		{
			OnUsed?.Invoke();
		}
	}
}
