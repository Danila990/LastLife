using System;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic.Interactions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor
{
	public class EnvironmentCollider : MonoBehaviour
	{
		public Collider Collider;
		public DamageBehaviourData DBD;
		public EnviromentProjectileInteraction Interaction;

		[Button]
		private void Remove()
		{
			DestroyImmediate(this);
		}
		
		[Button, TitleGroup("DBD")]
		public void AsMetal()
		{
			Init();
			SetDBD(@"Assets/Settings/Data/Characters/DamageBehaviour/EnvDBD/MetalMediumDBD.asset");
		}
		
		[Button, TitleGroup("DBD")]
		public void AsDefault()
		{
			Init();
			SetDBD(@"Assets/Settings/Data/Characters/DamageBehaviour/BaseDamageBehaviourData.asset");
		}

		private void Init()
		{
			Action<Transform> setLayerMask = (obj) => obj.gameObject.layer = Layers.EnvironmentLayer; 
			RecursiveCycle(transform, setLayerMask);
		}
		private void SetDBD(string path)
		{
			Interaction = GetOrAdd<EnviromentProjectileInteraction>();
			Interaction.BehaviourData = AssetDatabase.LoadAssetAtPath<DamageBehaviourData>(path);
			DBD = Interaction.BehaviourData;
		}
		
		[Button, TitleGroup("Colliders")]
		private void SetBoxCollider()
		{
			var renderer = transform.GetComponent<MeshRenderer>();
			var boxCollider = GetOrAdd<BoxCollider>();
			Collider = boxCollider;
			boxCollider.center = renderer.bounds.center;
			boxCollider.size = renderer.bounds.size;
		}
		[Button, TitleGroup("Colliders")]

		private void SetMeshCollider()
		{
			var filter = transform.GetComponent<MeshFilter>();
			var meshCollider = GetOrAdd<MeshCollider>();
			Collider = meshCollider;
			meshCollider.sharedMesh = filter.sharedMesh;
		}

		private T GetOrAdd<T>() where T : Component
		{
			if(transform.TryGetComponent(out T component))
				return component;

			return transform.gameObject.AddComponent<T>();
		}
		
		private void RecursiveCycle(Transform root, Action<Transform> action)
		{
			action(root);
			foreach (Transform child in root)
			{
				RecursiveCycle(child, action);
				action(child);
			}
		}
	}
}
