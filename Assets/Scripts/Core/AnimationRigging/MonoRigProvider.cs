using System;
using System.Collections.Generic;
using System.Linq;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Core.AnimationRigging
{
	public class MonoRigProvider : MonoBehaviour, IRigProvider
	{
		[SerializeField] private RigKeyValue[] _rigSetupData;
		public IReadOnlyDictionary<string, RigElementController> Rigs { get; private set; }
		
		private void Awake()
		{
			Rigs = _rigSetupData.ToDictionary(key => key.Key, value => new RigElementController(value.Data, value.Data.Target).AddTo(this));
		}

		public void SetRigStatus(bool status,string rigname)
		{
			var rig = Rigs[rigname];
			if (status)
			{
				rig.EnableRig();
				return;
			}
			rig.DisableRig();
		}

#if UNITY_EDITOR
		/// <summary>
		/// Spine, Spine1, Spine2, Head, ShootOrigin
		/// </summary>

		[Button]
		public void SetupRig()
		{
			var animator = GetComponentInChildren<Animator>();
			var rigBuilder = GetComponentInChildren<RigBuilder>();
			var upperChest = animator.GetBoneChildDeepth(HumanBodyBones.Spine,2);
			var hintL = CreateHelpTransform("HintL", upperChest, Vector3.left);
			var hintR = CreateHelpTransform("HintR", upperChest, -Vector3.left);
			var ikTargetL = CreateHelpTransform("IKTargetL", transform, Vector3.zero);
			var ikTargetR = CreateHelpTransform("IKTargetR", transform, Vector3.zero);
			var aimTarget = CreateHelpTransform("AimTarget", transform, Vector3.zero);
			var aimRigHolder = CreateHelpTransform("AimRigHolder", animator.transform, Vector3.zero);
			var IKLRigHolder = CreateHelpTransform("IKLRig", animator.transform, Vector3.zero);
			var IKRRigHolder = CreateHelpTransform("IKRRig", animator.transform, Vector3.zero);
			var aimRig = aimRigHolder.AddComponent<Rig>();
			var IKLRig = IKLRigHolder.AddComponent<Rig>();
			var IKRRig = IKRRigHolder.AddComponent<Rig>();
			rigBuilder.layers.Clear();
			rigBuilder.layers.Add(new RigLayer(aimRig));
			rigBuilder.layers.Add(new RigLayer(IKLRig));
			rigBuilder.layers.Add(new RigLayer(IKRRig));
			foreach (var setup in AutoRigSetup)
			{
				var bone = animator.GetBoneChildDeepth(setup.RefBone, setup.ChildDepth);
				var constrainObj = CreateHelpTransform(setup.Name+" Constrain", aimRigHolder.transform, Vector3.zero);
				var constrain = constrainObj.AddComponent<MultiAimConstraint>();
				constrain.weight = setup.ConstrainWeight;
				constrain.data.constrainedObject = bone;
				constrain.data.sourceObjects.Add(new WeightedTransform(aimTarget.transform,1));
				UnityEditor.EditorUtility.SetDirty(constrain);
			}

			var origin = CreateHelpTransform("Origin", animator.GetBoneChildDeepth(HumanBodyBones.Head, 0),
				Vector3.zero);
			var constrainObjOrigin = CreateHelpTransform("Origin Constrain", aimRigHolder.transform, Vector3.zero);
			var constrainOrigin = constrainObjOrigin.AddComponent<MultiAimConstraint>();
			constrainOrigin.weight = 1;
			constrainOrigin.data.constrainedObject = origin.transform;
			constrainOrigin.data.sourceObjects.Add(new WeightedTransform(aimTarget.transform,1));
			UnityEditor.EditorUtility.SetDirty(constrainOrigin);
			MakeIkConstrain(IKLRigHolder.transform,hintL.transform,HumanBodyBones.LeftUpperArm,animator,ikTargetL.transform);
			MakeIkConstrain(IKRRigHolder.transform,hintR.transform,HumanBodyBones.RightUpperArm,animator,ikTargetR.transform);
			var aimData = new RigKeyValue
			{
				Key = "aim", Data = new RigData
				{
					Rig = aimRig, Target = aimTarget.transform,DisableDelay = 0,DisableTime = .3f,EnableDelay = 0,EnableTime = .3f
				}
			};
			var iklData = new RigKeyValue
			{
				Key = "secondGrip", Data = new RigData
				{
					Rig = IKLRig, Target = ikTargetL.transform,DisableDelay = 0,DisableTime = .3f,EnableDelay = 0,EnableTime = .3f
				}
			};
			var ikrData = new RigKeyValue
			{
				Key = "firstGrip", Data = new RigData
				{
					Rig = IKRRig, Target = ikTargetR.transform,DisableDelay = 0,DisableTime = .3f,EnableDelay = 0,EnableTime = .3f
				}
			};
			_rigSetupData = new RigKeyValue[3];
			_rigSetupData[0] = aimData;
			_rigSetupData[1] = iklData;
			_rigSetupData[2] = ikrData;
			UnityEditor.EditorUtility.SetDirty(this);
		}

		private void MakeIkConstrain(Transform holder,Transform hint,HumanBodyBones startBone,Animator animator,Transform target)
		{
			var constrainIkObj = CreateHelpTransform("IK Constrain", holder, Vector3.zero);
			var Ik = constrainIkObj.AddComponent<TwoBoneIKConstraint>();
			Ik.weight = 1;
			Ik.data.hint = hint;
			Ik.data.root = animator.GetBoneChildDeepth(startBone, 0);
			Ik.data.mid = animator.GetBoneChildDeepth(startBone, 1);
			Ik.data.tip = animator.GetBoneChildDeepth(startBone, 2);
			Ik.data.target = target;
			UnityEditor.EditorUtility.SetDirty(Ik);
		}

		private GameObject CreateHelpTransform(string name, Transform parent, Vector3 pos)
		{
			var obj = new GameObject();
			obj.name = name;
			obj.transform.SetParent(parent);
			obj.transform.localPosition = pos;
			UnityEditor.EditorUtility.SetDirty(obj);
			return obj;
		}

		private struct AutoRigData
		{
			public HumanBodyBones RefBone;
			public int ChildDepth;
			public float ConstrainWeight;
			public string Name;
		}
		
		private static AutoRigData[] AutoRigSetup =
		{
			new AutoRigData{RefBone = HumanBodyBones.Spine,ChildDepth = 0,ConstrainWeight = 0.25f,Name = "Spine"},
			new AutoRigData{RefBone = HumanBodyBones.Spine,ChildDepth = 1,ConstrainWeight = 0.5f,Name = "Chest"},
			new AutoRigData{RefBone = HumanBodyBones.Spine,ChildDepth = 2,ConstrainWeight = 0.75f,Name = "UpperChest"},
			new AutoRigData{RefBone = HumanBodyBones.Head,ChildDepth = 0,ConstrainWeight = 1f,Name = "Head"},
		};
		
		//[Button]
		/*public void SetupAimRig()
		{
			var rigData = _rigSetupData.FirstOrDefault(value => value.Key == "aim");
			if (rigData.Key != "aim")
			{
				Debug.LogError("rigData invalid " + rigData);
				return;
			}
			var animator = GetComponentInChildren<Animator>();

			for (int i = 0; i < EditorSetupData.Length && rigData.Data.Rig.transform.childCount > i; i++)
			{
				var child = rigData.Data.Rig.transform.GetChild(i);
				var rigd = EditorSetupData[i];
				if (!child.TryGetComponent(out MultiAimConstraint multiAimConstraint))
				{
					multiAimConstraint = child.gameObject.AddComponent<MultiAimConstraint>();
				}
				if (rigd.HumanBodyBones == HumanBodyBones.RightEye)
				{
					var head = animator.GetBoneTransform(HumanBodyBones.Head);
					var shootOrigin = new GameObject("ShootOrigin").transform;
					shootOrigin.parent = head;
					multiAimConstraint.data.constrainedObject = shootOrigin;
					UnityEditor.EditorUtility.SetDirty(shootOrigin);
				}
				else
				{
					multiAimConstraint.data.constrainedObject = animator.GetBoneTransform(rigd.HumanBodyBones);
				}
				multiAimConstraint.data.sourceObjects.SetTransform(0, AimTarget);
				
				UnityEditor.EditorUtility.SetDirty(child);
			}
			UnityEditor.EditorUtility.SetDirty(this);
		}

		private static RigEditorSetupData[] EditorSetupData =
		{
			new RigEditorSetupData {HumanBodyBones = HumanBodyBones.Spine },
			new RigEditorSetupData {HumanBodyBones = HumanBodyBones.Chest },
			new RigEditorSetupData {HumanBodyBones = HumanBodyBones.UpperChest },
			new RigEditorSetupData {HumanBodyBones = HumanBodyBones.Head },
			new RigEditorSetupData {HumanBodyBones = HumanBodyBones.RightEye },
		};
		
		private struct RigEditorSetupData
		{
			public HumanBodyBones HumanBodyBones;
			public float DefaultAimWeight;
		}*/
#endif

		[Serializable]
		public struct RigKeyValue
		{
			public string Key;
			public RigData Data;
		}
	}
}