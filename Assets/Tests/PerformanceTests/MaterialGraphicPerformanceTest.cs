using System.Collections;
using Cysharp.Threading.Tasks;
using SharedUtils;
using Tests.UITest;
using Unity.PerformanceTesting;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PerformanceTests
{
	public class MaterialGraphicPerformanceTest : UiTest
	{
		protected override void OverrideRegistration()
		{
			Application.targetFrameRate = -1;
		}
		
		[UnityTest, Performance, Version("Graphic Test 2")]
		public IEnumerator DefaultRendererTest() => UniTask.ToCoroutine(async () =>
		{
			var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/3d/Environment/MarketMap/CarRepair.fbx");
			var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/FX/New Material.mat");
			
			var outlineFillMaterial = Object.Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));
			var renderers = model.GetComponentsInChildren<Renderer>();
			
			// foreach (var renderer in renderers)
			// {
			// 	renderer.material = mat;
			// }
			
			for (int i = 0; i < 50; i++)
				for (int j = 0; j < 50; j++)
				{
					var createdModel = Object.Instantiate(model);
					createdModel.transform.position = new Vector3(i % 2 == 0 ? i * 5 : i * -5,i,57.8f);
				}
			
			var cameraGO = new GameObject("name", typeof(Camera));
			
			await UniTask.Delay(1f.ToSec());
			await Measure
				.Frames()
				.MeasurementCount(120)
				.WarmupCount(10)
				.Run()
				.ToUniTask(cancellationToken:CancellationToken);
		});
	}
}