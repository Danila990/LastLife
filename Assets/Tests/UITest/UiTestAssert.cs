#if UNITY_EDITOR

using System.Collections;
using System.Linq;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.InputSystem;
using Tests.UITest.Conditions;
using Tests.UITest.Custom.Conditions;
using Tests.UITest.Utils;
using UnityEngine;
using VContainer;
using VContainerUi.Interfaces;

namespace Tests.UITest
{
	public abstract partial class UiTest
	{
		protected static IEnumerator AssertLabel(string id, string text)
		{
			return AssertLabelInternal(id, text);
		}

		protected static IEnumerator AssertToggle(string id, bool val)
		{
			return AssertToggleInternal(id, val);
		}

		protected static IEnumerator AssertSlider(string id, float val)
		{
			return AssertSliderValueInternal(id, val);
		}

		protected static IEnumerator AssertObjectEulerRotationZ(string id, Interval val)
		{
			return AssertObjectEulerRotationZInternal(id, val);
		}

		protected static IEnumerator AssertObjectActive(string id)
		{
			return ObjectActive(id);
		}

		protected static IEnumerator AssertObjectInactive(string id)
		{
			return ObjectInactive(id);
		}

		protected static IEnumerator AssertObjectArrayActive(params string[] ids)
		{
			foreach (var id in ids)
			{
				yield return AssertObjectActive(id);
			}

			yield return null;
		}

		protected static IEnumerator AssertObjectArrayInactive(params string[] ids)
		{
			foreach (var id in ids)
			{
				yield return AssertObjectInactive(id);
			}

			yield return null;
		}

		protected static IEnumerator AssertUiViewInactive<TView>(bool projectScope = false, float timeout = 2) where TView : MonoBehaviour, IUiView
		{
			return WaitFor(new UiViewInactive<TView>(projectScope), timeout);
		}
		
		protected static IEnumerator AssertUiViewActive<TView>(bool projectScope = false, float timeout = 2) where TView : MonoBehaviour, IUiView
		{
			return WaitFor(new UiViewActive<TView>(projectScope), timeout);
		}
		
		protected static IEnumerator AssertUiViewActive<TView>(Canvas canvas, float timeout = 2) where TView : MonoBehaviour, IUiView
		{
			return WaitFor(new UiViewActive<TView>(canvas), timeout);
		}
		
		private static IEnumerator AssertLabelInternal(string id, string text)
		{
			yield return WaitFor(new LabelTextAppeared(id, text));
		}

		private static IEnumerator AssertToggleInternal(string id, bool val)
		{
			yield return WaitFor(new ToggleValueAppeared(id, val));
		}

		private static IEnumerator AssertSliderValueInternal(string id, float val)
		{
			yield return WaitFor(new AssertSliderValue(id, val));
		}

		private static IEnumerator AssertObjectEulerRotationZInternal(string id, Interval val)
		{
			yield return WaitFor(new ObjectRotationZCondition(id, val));
		}
		
		protected static void DestroyAllLifeScene(IObjectResolver objectResolver, bool deactivatePlayer = false)
		{
			var entityRepository = objectResolver.Resolve<IEntityRepository>();
			var arr = entityRepository.EntityContext
				.Where(x => x is LifeEntity lifeEntity)
				.Where(x => x is CharacterContext { Adapter: not PlayerCharacterAdapter })
				.ToArray();

			foreach (var entity in arr)
			{
				entity.OnDestroyed(entityRepository);
				Object.Destroy(entity);
			}
			
			if (deactivatePlayer)
			{
				var playerSpawnService = objectResolver.Resolve<IPlayerSpawnService>();
				playerSpawnService.PlayerCharacterAdapter.gameObject.SetActive(false);
			}
		}
	}
}
#endif