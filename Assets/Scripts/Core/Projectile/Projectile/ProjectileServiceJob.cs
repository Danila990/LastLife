// using System;
// using System.Collections.Generic;
// using Game.Factories;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;
// using UnityEngine.Jobs;
// using VContainer.Unity;
//
// namespace Game.Services.Projectile
// {
//
// 	[BurstCompile]
// 	public struct MoveProjectileJob : IJobParallelForTransform
// 	{
// 		public NativeArray<ProjectileController> Controllers;
//
// 		[ReadOnly]
// 		public float DeltaTime;
// 		[ReadOnly]
// 		public Vector3 Graivity;
// 		
// 		[BurstCompile]
// 		public void Execute(int index, TransformAccess transform)
// 		{
// 			var controller = Controllers[index];
// 			controller.Position += controller.Direction * DeltaTime;
// 			controller.Direction -= Graivity * DeltaTime;
// 			transform.position = controller.Position;
// 			transform.rotation = Quaternion.LookRotation(controller.Direction.normalized);
// 		}
// 	}
// 	public class ProjectileServiceJob : ITickable, IDisposable, IProjectileService
// 	{
// 		private readonly List<ProjectileLink> _links = new List<ProjectileLink>();
// 		public SandboxProjectileFactory Factory;
// 		
// 		private JobHandle _jobHandle;
// 		private NativeArray<RaycastHit> _raycastHit = new NativeArray<RaycastHit>(0, Allocator.Persistent);
// 		private NativeArray<RaycastCommand> _raycastCommands = new NativeArray<RaycastCommand>(0, Allocator.Persistent);
// 		
// 		public void CreateProjectile(
// 			ProjectileCreationData creationData)
// 		{
// 			var link = new ProjectileLink(
// 				creationData, 
// 				creationData.ProjectileView, 
// 				new ProjectileController(
// 					creationData.Position,
// 					creationData.Velocity));
// 			
// 			_links.Add(link);
// 		}
// 		
// 		public void Tick()
// 		{
// 			var bulletCount = _links.Count;
// 			var controllers =
// 				new NativeArray<ProjectileController>(bulletCount, Allocator.TempJob);
//
// 			var positions =
// 				new NativeArray<Vector3>(bulletCount, Allocator.TempJob);
//
// 			var transforms = new TransformAccessArray(bulletCount);
// 			
// 			for (int i = 0; i < bulletCount; i++)
// 			{
// 				var bulletLink = _links[i];
//
// 				controllers[i] = bulletLink.Controller;
// 				positions[i] = bulletLink.Controller.Position;
// 				transforms[i] = bulletLink.View.transform;
// 			}
// 			
// 			MoveProjectileJob projectileJob = new MoveProjectileJob()
// 			{
// 				Controllers = controllers,
// 				DeltaTime = Time.deltaTime,
// 				Graivity = Physics.gravity
// 			};
//
// 			projectileJob.Schedule(transforms).Complete();
//
// 			controllers.Dispose();
// 			positions.Dispose();
// 			transforms.Dispose();
// 		}
//
// 		public void Dispose()
// 		{
// 		}
// 	}
// }

