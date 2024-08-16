using System;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Db.ObjectData;
using GameSettings;
using MessagePipe;
using PriorityQueues;
using Utils.Constants;
using VContainer.Unity;

namespace Core.Services
{
	public class CircularCharacterDestroyingService : IDisposable, IInitializable
	{
		private readonly ISubscriber<RepositoryEvent> _repEvt;
		private readonly ISettingsService _settingsService;
		private readonly MappedBinaryPriorityQueue<EntityContext> _contexts = new MappedBinaryPriorityQueue<EntityContext>(30, Comparison);
		private IDisposable _disposable;

		public CircularCharacterDestroyingService(
			ISubscriber<RepositoryEvent> repEvt, 
			ISettingsService settingsService
			)
		{
			_repEvt = repEvt;
			_settingsService = settingsService;
		}
		
		public void Initialize()
		{
			_disposable = _repEvt.Subscribe(OnRepEvent);
		}
		
		private void OnRepEvent(RepositoryEvent repEvt)
		{
			if (repEvt is not { GenericEntity: true, EntityContext: CharacterContext { CurrentAdapter: AiCharacterAdapter}})
				return;
			
			if (repEvt.EvtType == RepositoryEvent.EventType.Add)
			{
				_contexts.Enqueue(repEvt.EntityContext);
				var maxCount = _settingsService.QualityPreset.SelectedPreset.Value.GetValue<int>(SettingsConsts.MAX_BOT_SPAWNED_COUNT, GameSetting.ParameterType.Int);
				while(_contexts.Count >= maxCount && _contexts.Count > 0)
				{
					var toDelete = _contexts.Dequeue();
					if (toDelete is CharacterContext { CurrentAdapter: AiCharacterAdapter aiCharacterAdapter})
					{
						aiCharacterAdapter.DeathTask().Forget();
					}
				}
			}
			else if (_contexts.Contains(repEvt.EntityContext))
			{
				_contexts.Remove(repEvt.EntityContext);
			}
		}
		
#region Compare
		private static int Comparison(EntityContext x, EntityContext y)
		{
			if (ReferenceEquals(x, y))
				return 0;
			if (ReferenceEquals(null, y))
				return 1;
			if (ReferenceEquals(null, x))
				return -1;
				
			var uidCompare = x.Uid.CompareTo(y.Uid);
			if (uidCompare != 0)
				return uidCompare;
				
			var nameComparison = string.Compare(x.name, y.name, StringComparison.Ordinal);
			if (nameComparison != 0)
				return nameComparison;
			var hideFlagsComparison = x.hideFlags.CompareTo(y.hideFlags);
			if (hideFlagsComparison != 0)
				return hideFlagsComparison;
			var tagComparison = string.Compare(x.tag, y.tag, StringComparison.Ordinal);
			if (tagComparison != 0)
				return tagComparison;
			var enabledComparison = x.enabled.CompareTo(y.enabled);
			if (enabledComparison != 0)
				return enabledComparison;
			var isActiveAndEnabledComparison = x.isActiveAndEnabled.CompareTo(y.isActiveAndEnabled);
			if (isActiveAndEnabledComparison != 0)
				return isActiveAndEnabledComparison;
			var useGUILayoutComparison = x.useGUILayout.CompareTo(y.useGUILayout);
			if (useGUILayoutComparison != 0)
				return useGUILayoutComparison;
			#if UNITY_EDITOR
			var runInEditModeComparison = x.runInEditMode.CompareTo(y.runInEditMode);
			if (runInEditModeComparison != 0)
				return runInEditModeComparison;
			#endif
			var uidComparison = x.Uid.CompareTo(y.Uid);
			return uidComparison != 0 ? uidComparison : x.IsDestroyed.CompareTo(y.IsDestroyed);
		}
#endregion

		public void Dispose()
		{
			_disposable?.Dispose();
		}

		
	}
}