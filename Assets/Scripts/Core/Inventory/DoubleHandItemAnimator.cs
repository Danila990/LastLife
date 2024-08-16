using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using UnityEngine;

namespace Core.Inventory
{
	public class DoubleHandItemAnimator : SimpleItemAnimator
	{
		[SerializeField] private Transform _extraModelPrefab;
		[SerializeField] private HumanBodyBones _extraModelBone;
		[SerializeField] private Vector3 _extraOffset;
		[SerializeField] private Vector3 _extraRotate;
		[SerializeField] private Vector3 _extraScale;
		[SerializeField] private Vector3 _extraFpvRotate;
		[SerializeField] private Vector3 _extraFpvOffset;
		[SerializeField] private Vector3 _extraFpvScale;
		
		private Transform _extraModel;
		private Transform _extraModelHolder;
		public GameObject ExtraModel => _extraModel.gameObject;
		
		public override void OnAdapterSet(IEntityAdapter adapter)
		{
			base.OnAdapterSet(adapter);
			if (adapter.Entity is CharacterContext context)
			{
				_extraModelHolder = context.CharacterAnimator.Animator.GetBoneTransform(_extraModelBone);
			}
		}

		public override void SpawnModel()
		{
			base.SpawnModel();
			if (!_extraModel && _extraModelPrefab)
			{
				_extraModel = Instantiate(_extraModelPrefab);
				_extraModel.gameObject.SetActive(false);
			}
		}

		public override void EquipModel()
		{
			base.EquipModel();
			
			if (ViewSettings.CanTPV && _extraModel)
			{
				_extraModel.gameObject.SetActive(true);
				_extraModel.transform.SetParent(_extraModelHolder);
				_extraModel.transform.SetLocalPositionAndRotation(_extraOffset,Quaternion.Euler(_extraRotate));
				_extraModel.transform.localScale = _extraScale;
			}
			if (ViewSettings.CanFPV && !IsTpvMode)
			{
				_extraModel.gameObject.SetActive(true);
				_extraModel.transform.SetParent(!ViewSettings.PlaceInLeft ? Adapter.FpvHandsAnimator.FpvCam.FPVHands.LeftHand : Adapter.FpvHandsAnimator.FpvCam.FPVHands.RightHand);
				_extraModel.transform.SetLocalPositionAndRotation(_extraFpvOffset,Quaternion.Euler(_extraFpvRotate));
				_extraModel.transform.localScale = _extraFpvScale;
			}
		}
		
		public override void UnEquipModel()
		{
			base.UnEquipModel();
			if (_extraModel)
			{
				_extraModel.gameObject.SetActive(false);
			}
		}
	}
}