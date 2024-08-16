using Core.CameraSystem;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Core.Inventory.Origins
{

	
	
	public class CharacterOriginProvider : BaseOriginProvider
	{
		[Required] public CharacterContext CharacterContext;
		[Required] public Transform CharacterShootOrigin;
		[Inject] private readonly ICameraService _cameraService;
		private bool IsPlayerOwned => CharacterContext.CurrentAdapter is PlayerCharacterAdapter;
		
		public override Transform GetOrigin(string id)
		{
			if (!IsPlayerOwned)
				return CharacterShootOrigin;
			
			return _cameraService.IsThirdPerson ? CharacterShootOrigin : _cameraService.FpvCam.transform;
		}

		public override Transform GetStaticOrigin()
		{
			return CharacterShootOrigin;
		}
		
		public override Transform GetOrigin(string aim, ItemContext itemContext)
		{
#if UNITY_EDITOR
			if (!EditorApplication.isPlaying)
			{
				return CharacterShootOrigin;
			}
#endif
			if (!IsPlayerOwned)
			{
				return CharacterShootOrigin;
			}
            
			if (_cameraService.IsThirdPerson)
			{
				return CharacterShootOrigin;
			}
			
			if (itemContext.ItemAnimator && itemContext.ItemAnimator.RuntimeModel)
			{
				var model = itemContext.ItemAnimator.RuntimeModel;
				model.Origin.forward = _cameraService.FpvCam.transform.forward;
				return model.Origin;
			}
			
			return CharacterShootOrigin;
		}
		
		public override Vector3 GetMeleeOrigin()
		{
			if (IsPlayerOwned)
			{
				if (_cameraService.IsThirdPerson)
				{
					return CharacterContext.MainTransform.position + CharacterContext.MainTransform.rotation * new Vector3(0, 1, 1.25f);
				}
				else
				{
					var t = _cameraService.CurrentBrain.transform;
					return t.position + t.rotation * Vector3.forward;
				}
			}
			else
			{
				return CharacterContext.MainTransform.position + CharacterContext.MainTransform.rotation * new Vector3(0, 1, 1.25f);
			}
		}
	}
}