#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using Core.Entity.Characters;
using ControlFreak2;
using Core.Entity.Characters.Adapters;
using Core.Factory;
using Core.InputSystem;
using Core.Inventory.Items;
using Core.Inventory.Items.Weapon;
using Core.Services;
using Cysharp.Threading.Tasks;
using GameStateMachine.States.Impl.Project;
using MessagePipe;
using Tests.UITest.Conditions;
using Tests.UITest.Utils;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Tests.UITest
{
	public abstract partial class UiTest : Custom.BaseTest
	{

		public static IEnumerator LoadScene(string name)
		{
			return LoadSceneInternal(name);
		}

		private static IEnumerator LoadSceneInternal(string name)
		{
#if UNITY_EDITOR
			if (name.Contains(".unity"))
			{
				EditorSceneManager.LoadSceneInPlayMode(name, new LoadSceneParameters(LoadSceneMode.Single));
				yield return WaitFor(new SceneLoaded(Path.GetFileNameWithoutExtension(name)));
				yield break;
			}
#endif
			SceneManager.LoadScene(name);
			yield return WaitFor(new SceneLoaded(name));
		}


		public static IEnumerator Press(string buttonName)
		{
			return PressInternal(buttonName);
		}

		public static IEnumerator WaitWhile(Func<bool> condition, float timeout = 2f)
		{
			yield return WaitUntil(() => !condition(), timeout);
		}
		
		public static IEnumerator WaitUntil(Func<bool> condition, float timeout = 2f)
		{
			yield return WaitFor(new BoolCondition(condition), timeout);
		}

		public async static UniTask<LifetimeScope> InitSandbox(CancellationToken token)
		{
			return  await InitScene(SceneLoaderService.SANDBOX,token);
		}
		
		public async static UniTask<LifetimeScope> InitMarket(CancellationToken token)
		{
			return await InitScene(SceneLoaderService.MARKET,token);
		}
		
		public async static UniTask<LifetimeScope> InitScene(string sceneName, CancellationToken token)
		{
			await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).ToUniTask(cancellationToken: token);
			return await InitScope(token);
		}

		public async static UniTask<LifetimeScope> LoadThroughGameScene(string sceneName, CancellationToken token)
		{
			var lifetimeScope = DiUtils.SceneScope;
			var stateMachine = lifetimeScope.Container.Resolve<ProjectStateMachine>();
			var sub = lifetimeScope.Container.Resolve<ISubscriber<SceneLoadedMessage>>();
			stateMachine.ChangeStateAsync<ProjectLoadSceneState, string>(sceneName);
			await sub.FirstAsync(token);
			return DiUtils.SceneScope;
		}
		
		private async static UniTask<LifetimeScope> InitScope(CancellationToken token)
		{
			var sceneScope = DiUtils.SceneScope;
			sceneScope.Build();
		    await sceneScope.Container.Resolve<ISubscriber<SceneLoadedMessage>>().FirstAsync(token);
			return sceneScope;
		}
		
		public static IEnumerator Press(GameObject o)
		{
			return PressInternal(o);
		}

		public static IEnumerator ObjectActive(string id)
		{
			var appeared = new ObjectAppeared(id);
			yield return WaitFor(appeared);
		}

		public static IEnumerator ObjectInactive(string id)
		{
			var disappeared = new ObjectDisappeared(id);
			yield return WaitFor(disappeared);
		}

		public static IEnumerator PressInternal(string buttonName)
		{
			var buttonAppeared = new ObjectAppeared(buttonName);
			yield return WaitFor(buttonAppeared);
			yield return Press(buttonAppeared.Obj);
		}

		private static IEnumerator PressInternal(GameObject o)
		{
			if (o.GetComponent<Button>() == null)
			{
				o = o.GetComponentInChildren<Button>().gameObject;
			}
			yield return WaitFor(new ButtonAccessible(o));
			Debug.Log("Button pressed: " + o);
			ExecuteEvents.Execute(o, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
			yield return null;
		}

		public static IEnumerator WaitFor(ICondition condition, float timeout = 2)
		{
			return WaitForInternal(condition, timeout);
		}

		private static IEnumerator WaitForInternal(ICondition condition, float timeout)
		{
			return new WaitCondition(condition, timeout).GetEnumerator();
		}

		public static AiCharacterAdapter CreateDummy(IAdapterStrategyFactory strategyFactory,Vector3 pos)
		{
			var pistolMan = strategyFactory.CreateAiAdapter<AiCharacterAdapter>("PistolMan", pos);
			pistolMan.Pause();
			return pistolMan;
		}
		
		public static T FindFirstPlayerItem<T>(CharacterContext characterContext)
			where T : ItemContext
		{
			return (T)characterContext.Inventory.InventoryItems.FirstOrDefault(x => x.ItemContext is T).ItemContext;
		}
		
		public static void SelectWeapon(CharacterContext characterContext)
		{
			var weapon = FindFirstPlayerItem<ProjectileWeaponContext>(characterContext);
			characterContext
				.Inventory
				.SelectItem(weapon);
		}
		
		public static async UniTask FakeUserInput(CancellationToken token)
		{
			var vertAxis = CF2Input.activeRig.axes.Get("Vertical");
			while (!token.IsCancellationRequested)
			{
				vertAxis.SetNormalizedDelta(1);
				vertAxis.SetDigital();
				await UniTask.Yield(cancellationToken: token);
			}
		}
		
		public static async UniTask FakeUserInput(CancellationToken token,float time)
		{
			var vertAxis = CF2Input.activeRig.axes.Get("Vertical");
			while (!token.IsCancellationRequested)
			{
				vertAxis.SetNormalizedDelta(1);
				vertAxis.SetDigital();
				await UniTask.Yield(cancellationToken: token);
				time -= Time.deltaTime;
				if(time<0) return;
			}
		}
	}
}
#endif