using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

namespace Core.Equipment.Inventory
{
	public class CharacterRenderer : MonoBehaviour
	{
		[SerializeField] private GameObject _mannequin;
		[SerializeField] private EquipmentInventoryRenderer _equipmentRenderer;
		[SerializeField] private PlayableDirector _playableDirector;
		[SerializeField] private GameObject[] _additionalObjects;
		private bool _isUnlocked;
		public Transform Mannequin => _mannequin.transform;
		public EquipmentInventoryRenderer EquipmentRenderer => _equipmentRenderer;
		[field:SerializeField] public Animator Animator { get; private set; }
		
		public bool IsActive { get; private set; }

		public void Init(IObjectResolver resolver)
		{
			resolver.Inject(_equipmentRenderer);
		}
		
		public void Enable()
		{
			Mannequin.localEulerAngles = Vector3.zero;
			gameObject.SetActive(true);	
			IsActive = true;
			
			if (_playableDirector)
			{
				if (_isUnlocked)
				{
					_playableDirector.Stop();
					_playableDirector.enabled = false;
					_additionalObjects.ForEach(x => x.SetActive(false));
				}
				else
				{
					_additionalObjects.ForEach(x => x.SetActive(true));
					_playableDirector.enabled = true;
					_playableDirector.Play();
				}
			}
			Animator.Update(0.1f);
		}

		public void Disable()
		{
			if (_playableDirector)
			{
				_playableDirector.Stop();
				_playableDirector.enabled = false;
			}
			gameObject.SetActive(false);
			IsActive = false;
		}
		
		public void SetIsUnlocked(bool isUnlocked)
		{
			_isUnlocked = isUnlocked;
		}
	}
}