using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Effects
{
	public class CharacterMeshProvider : MeshProvider
	{
		[SerializeField] private Transform _fallbackBone;
		[SerializeField, TableList] private Bone[] _bones;

		public Transform GetBone(string boneName)
		{
			var bone = _bones.FirstOrDefault(x => x.Name == boneName);
			return bone.Transform ? bone.Transform : _fallbackBone;
		}
		
		#if UNITY_EDITOR
		[Button]
		private void GetBones()
		{
			//TODO:
			// Change string to UnityEngine.HumanBodyBones
			_fallbackBone = GameObjectUtils.FindObjectByName(gameObject, "Hips").transform;
			var bones = new List<Bone>();
			bones.Add(new Bone("Hips", GameObjectUtils.FindObjectByName(gameObject, "Hips").transform));
			bones.Add(new Bone("Spine", GameObjectUtils.FindObjectByName(gameObject, "Spine2").transform));
			bones.Add(new Bone("Head", GameObjectUtils.FindObjectByName(gameObject, "Head").transform));
			bones.Add(new Bone("RightFoot", GameObjectUtils.FindObjectByName(gameObject, "RightFoot").transform));
			bones.Add(new Bone("LeftFoot", GameObjectUtils.FindObjectByName(gameObject, "LeftFoot").transform));
			bones.Add(new Bone("LeftLeg", GameObjectUtils.FindObjectByName(gameObject, "LeftLeg").transform));
			bones.Add(new Bone("RightLeg", GameObjectUtils.FindObjectByName(gameObject, "RightLeg").transform));
			_bones = bones.ToArray();
		}
		
		#endif
		
		[Serializable]
		private struct Bone
		{
			public string Name;
			public Transform Transform;
			
			public Bone(string name, Transform transform)
			{
				Name = name;
				Transform = transform;
			}
		}
	}
}
