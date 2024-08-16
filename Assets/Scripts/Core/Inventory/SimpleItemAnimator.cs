using System;
using System.Collections.Generic;
using Core.AnimationRigging;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Core.Inventory
{

	public class SimpleItemAnimator : ItemAnimator
	{
		private readonly List<IKBindUpdater> _currentBinds = new();
		private ItemAnimationBehaviour.RuntimeItemData _runtimeItemData;

		private bool _needUpdateIK;
		public PlayerCharacterAdapter Adapter { get; set; }
		public bool IsTpvMode => Adapter.FpvHandsAnimator.IsTpvMode;
		public override ItemModel RuntimeModel => GetModel(IsTpvMode);
		public override ref ItemAnimationBehaviour.RuntimeItemData RuntimeItemData => ref _runtimeItemData;

		protected ItemModel FpvModel { get; private set; }
		protected ItemModel ThpModel { get; private set; }

		public override void OnAdapterSet(IEntityAdapter adapter)
		{
			PrepareRuntimeItemData();
			if (adapter is PlayerCharacterAdapter playerCharacterAdapter)
			{
				ModelHolder = playerCharacterAdapter.CurrentContext.CharacterAnimator.Animator.GetBoneTransform(ViewSettings.ItemBone);
				Adapter = playerCharacterAdapter;
				
				SpawnModel();
			}
		}

		private void PrepareRuntimeItemData()
		{
			var hands = new Dictionary<int, AnimationBehaviour>();
			var items = new Dictionary<int, AnimationBehaviour>();
			var behs = ViewSettings.Behaviours;
			
			foreach (var behaviour in behs)
			{
				behaviour.HashKey();
				switch (behaviour.AnimatorType)
				{
					case AnimatorSelect.HandsAnimator:
						hands.Add(behaviour.HashedKey, behaviour);
						break;
					case AnimatorSelect.ItemAnimator:
						items.Add(behaviour.HashedKey,behaviour);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
        
			_runtimeItemData = new ItemAnimationBehaviour.RuntimeItemData(ViewSettings.OverrideIdle, ViewSettings.NeedShowHands, behs, hands, items);
		}
		
		public override void PlayAnim(string key)
		{
			if (Adapter)
				Adapter.FpvHandsAnimator.PlayAnimation(key);
		}
		
		public override void PlayAnim(int hash)
		{
			if (Adapter)
				Adapter.FpvHandsAnimator.PlayAnimation(hash);
		}
		
		public override UniTask<bool> PlayAnimAsync(int animHash, string triggerId)
		{
			if (Adapter)
				return Adapter.FpvHandsAnimator.PlayAction(animHash, triggerId);
			
			return new UniTask<bool>(false);
		}
		
		public override void SetFloat(int hash, float value)
		{
			if (Adapter)
				Adapter.CharacterAnimatorAdapter.SetFloat(hash, value);
		}

		public override void OnUpdate(float deltaTime)
		{
			if(!Adapter)
				return;
			
			if (!IsTpvMode)
			{
				Adapter.FpvHandsAnimator.ItemAnimationBehaviour.Update(Time.deltaTime);
			}
			
			if(!_needUpdateIK)
				return;
			
			foreach (var bind in _currentBinds)
			{
				bind.UpdateBind();
			}
		}
		
		public override void SpawnModel()
		{
			if(!ThpModel && ViewSettings.ItemModel)
			{
				ThpModel = Instantiate(ViewSettings.ItemModel, ModelHolder);
				destroyCancellationToken.Register(DestroyCallback, ThpModel.gameObject);
				ThpModel.gameObject.SetActive(false);
			}
			if (!FpvModel && ViewSettings.SeparateFpvModel)
			{
				FpvModel = Instantiate(ViewSettings.FpvModel);
				destroyCancellationToken.Register(DestroyCallback, FpvModel.gameObject);
				FpvModel.gameObject.SetActive(false);
			}
		}
		
		private static void DestroyCallback(object obj)
		{
			if (obj is GameObject go)
			{
				Destroy(go);
			}
		}

		public ItemModel GetModel(bool isThirdPerson)
		{
			if (ViewSettings.CanTPV && isThirdPerson)
				return ThpModel;

			if (ViewSettings.CanFPV && !isThirdPerson)
			{
				return ViewSettings.SeparateFpvModel ? FpvModel : ThpModel;
			}

			return null;
		}
		
		public override void EquipModel()
		{
			PrepareModel();
			if (!ViewSettings.PlaceInHand)
			{
				AttachIKBind();
			}
			
			if (ViewSettings.CanTPV && ThpModel)
			{
				ThpModel.transform.SetParent(ModelHolder);
				ThpModel.gameObject.SetActive(true);
				ThpModel.transform.SetLocalPositionAndRotation(ViewSettings.ModelOffset,Quaternion.Euler(ViewSettings.ModelRotation));
				ThpModel.transform.localScale = Vector3.one;
			}
			if(!Adapter) return;
			if (ViewSettings.CanFPV && !IsTpvMode)
			{
				RuntimeModel.gameObject.SetActive(true);
				
				var parent = Adapter.FpvHandsAnimator.GetTransformForFPVModel(ViewSettings);
				RuntimeModel.transform.SetParent(parent);
				RuntimeModel.transform.SetLocalPositionAndRotation(ViewSettings.FpvModelOffset,Quaternion.Euler(ViewSettings.FpvModelRotation));
				RuntimeModel.transform.localScale = ViewSettings.FpvModelScale;
			}
		}
		
		public override void UnEquipModel()
		{
			ClearIKBind();
			
			if (ThpModel)
				ThpModel.gameObject.SetActive(false);
			
			if (FpvModel)
				FpvModel.gameObject.SetActive(false);
		}
		
		public override void TrySetAimState(bool state)
		{
			if (Adapter)
			{
				Adapter.SetAimState(state ? AimState.Aim : AimState.Default);
			}
		}

		private void AttachIKBind()
		{
			ClearIKBind();
			if(ViewSettings.GripType == ItemIKGripType.None) 
				return;
			
			_needUpdateIK = true;

			if (ViewSettings.GripType == ItemIKGripType.OneBone)
			{
				CreateBind("firstGrip", RuntimeModel.FirstGripPoint);
				EnableRig("firstGrip");
			}
			
			if (ViewSettings.GripType == ItemIKGripType.TwoBones)
			{
				CreateBind("firstGrip", RuntimeModel.FirstGripPoint);
				EnableRig("firstGrip");
				
				CreateBind("secondGrip", RuntimeModel.SecondGripPoint);
				EnableRig("secondGrip");
			}
		}
		
		
		private void ClearIKBind()
		{
			_currentBinds.Clear();
			_needUpdateIK = false;
			if (ViewSettings.GripType == ItemIKGripType.None)
				return;
			
			if (ViewSettings.GripType == ItemIKGripType.OneBone)
			{
				DisableRig("firstGrip");
			}
			
			if (ViewSettings.GripType == ItemIKGripType.TwoBones)
			{
				DisableRig("firstGrip");
				DisableRig("secondGrip");
			}
		}

		private void CreateBind(string rigName, Transform bind)
		{
			var rigTarget = GetRigProvider().Rigs[rigName].RigData.Target;
			var ikBind = new IKBindUpdater
			{
				IKTarget = rigTarget,
				IKBindTarget = bind,
				PosOffset = IsTpvMode ? Vector3.up * 0.1f : Vector3.zero,
				RotOffset = Quaternion.AngleAxis(90, Vector3.right)
			};
			_currentBinds.Add(ikBind);
		}

		public void PrepareModel()
		{
			if(!Adapter) return;
			SpawnModel();
			Adapter.FpvHandsAnimator.FpvCam.FPVHands.MeshRenderer.enabled = (!IsTpvMode && ViewSettings.UseFpvHands);
			if (ViewSettings.UseFpvHands)
			{
				DelayMoveFpvHands().Forget();
			}
		}

		private async UniTaskVoid DelayMoveFpvHands()
		{
			var ct = Adapter.FpvHandsAnimator.GetCancellationTokenOnDestroy();
			Adapter.FpvHandsAnimator.FpvCam.FPVHands.transform.localPosition = Vector3.up * 1000;
			await UniTask.NextFrame(ct);
			Adapter.FpvHandsAnimator.EquipItem(ViewSettings.FpvHandPos, ViewSettings.FpvHandScale);
		}
		
		private IRigProvider GetRigProvider() => IsTpvMode ? Adapter.CurrentContext.RigProvider : Adapter.FpvHandsAnimator.RigProvider;
		private void EnableRig(string rigName) => Adapter.CurrentContext.RigProvider.Rigs[rigName].EnableRig();
		private void DisableRig(string rigName) => Adapter.CurrentContext.RigProvider.Rigs[rigName].DisableRig();

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if(!ModelHolder) 
				return;
			
			Gizmos.DrawLine(ModelHolder.position, ModelHolder.TransformPoint(ViewSettings.ModelOffset));
			Gizmos.DrawWireSphere(ModelHolder.TransformPoint(ViewSettings.ModelOffset),0.05f);
		}
#endif
	}
}