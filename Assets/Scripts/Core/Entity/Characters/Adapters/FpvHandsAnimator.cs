using Core.AnimationRigging;
using Core.CameraSystem;
using Core.HealthSystem;
using Core.Inventory;
using Core.Inventory.Items;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Utils.Constants;

namespace Core.Entity.Characters.Adapters
{
	public class FpvHandsAnimator : MonoBehaviour
	{
		private ICameraService _cameraService;
		private FirstPersonCameraController _fpvCameraController;
		private PlayerCharacterAdapter _playerCharacterAdapter;
		private ItemAnimationBehaviour _itemAnimationBehaviour;

		public bool IsTpvMode => _cameraService.IsThirdPerson;
		public IRigProvider RigProvider => _fpvCameraController.FPVHands.RigProvider;
		public FirstPersonCameraController FpvCam => _fpvCameraController;
		public ItemAnimationBehaviour ItemAnimationBehaviour => _itemAnimationBehaviour;
		
		public void Init(ICameraService cameraService, PlayerCharacterAdapter playerCharacterAdapter)
		{
			_playerCharacterAdapter = playerCharacterAdapter;
			_cameraService = cameraService;
			_fpvCameraController = _cameraService.FpvCam;
			
			_itemAnimationBehaviour = new ItemAnimationBehaviour(new ItemAnimationBehaviour.ItemAnimData(
				FpvCam.ItemAnimation,
				FpvCam.FPVHands.HandAnimator, 
				FpvCam.FPVHands.LegAnimator, 
				FpvCam.FPVHands.RigProvider));
		}

		public void OnContextChanged(CharacterContext characterContext, bool created)
		{
			if (created)
			{
				if (characterContext.Inventory.SelectedItem && characterContext.Inventory.SelectedItem.ItemAnimator)
				{
					var data = characterContext.Inventory.SelectedItem.ItemAnimator.RuntimeItemData;
					_itemAnimationBehaviour.SetupFromData(ref data);
				}
				
				characterContext.Inventory.OnItemSelected.Subscribe(OnItemSelected).AddTo(characterContext);
				characterContext.Health.OnDeath.Subscribe(OnDied).AddTo(characterContext);
			}
			else
			{
				_itemAnimationBehaviour.Stop(false);
			}
		}
		
		private void OnDied(DiedArgs obj)
		{
			_fpvCameraController.FPVHands.gameObject.SetActive(false);
		}

		private void OnItemSelected(ItemContext itemContext)
		{
			_itemAnimationBehaviour.Pause(false);
			
			if (itemContext.ItemAnimator)
			{
				var data = itemContext.ItemAnimator.RuntimeItemData;
				_itemAnimationBehaviour.SetupFromData(ref data);
				if (data.NeedShowHands)
				{
					var hash = _playerCharacterAdapter.CurrentContext.CharacterAnimator.Animator.GetBool(AHash.MovingParameterHash) ? AHash.RunParameterHash : AHash.IdleParameterHash;
					_itemAnimationBehaviour.PlayAnim(hash);
				}
				else
				{
					_itemAnimationBehaviour.PlayAnim(_itemAnimationBehaviour.GetIdleHash());
					FpvCam.FPVHands.HandAnimator.Update(0.1f);
				}
			}
		}

		public void EquipItem(Vector3 fpvHandPos, Vector3 fpvHandScale)
		{
			if (_cameraService.IsThirdPerson)
				return;
			
			_fpvCameraController.FPVHands.gameObject.SetActive(true);
			_fpvCameraController.FPVHands.transform.localPosition = fpvHandPos;
			_fpvCameraController.FPVHands.transform.localScale = fpvHandScale;
			_fpvCameraController.FPVHands.LegMeshRenderer.material = _playerCharacterAdapter.CurrentContext.LegMaterial;
			_fpvCameraController.FPVHands.MeshRenderer.material = _playerCharacterAdapter.CurrentContext.BodyMaterial;
			
			_itemAnimationBehaviour.PlayAnim(_itemAnimationBehaviour.GetIdleHash());
			FpvCam.FPVHands.HandAnimator.Update(0.1f);
		}
		
		public Transform GetTransformForFPVModel(ItemViewPresentationSettings viewSettings)
		{
			if (viewSettings.PlaceInHand)
			{
				return viewSettings.PlaceInLeft ? _fpvCameraController.FPVHands.LeftHand : _fpvCameraController.FPVHands.RightHand;
			}
			else
			{
				return _fpvCameraController.ItemAnimation.transform;
			}
		}
		
		public void PlayAnimation(string anim)
		{
			_itemAnimationBehaviour.PlayAnim(Animator.StringToHash(anim));
		}
		
		public void PlayAnimation(int anim)
		{
			_itemAnimationBehaviour.PlayAnim(anim);
		}
		
		public async UniTask<bool> PlayAction(int hash, string triggerId)
		{
			var result = await _itemAnimationBehaviour.PlayAnimAsync(hash, triggerId);
			return result.status;
		}
	}
}