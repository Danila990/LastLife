using System.Collections.Generic;
using Common.SpawnPoint;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.Factory;
using Cysharp.Threading.Tasks;
using GameSettings;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Map
{
	public class MapAiSpawnPoints : AbstractMapExterior
	{
		public AiSpawnPoint[] SpawnPoints;
		
		private readonly List<IEntityAdapter> _entityAdapters = new List<IEntityAdapter>();
		[Inject] protected readonly IAdapterStrategyFactory AdapterStrategy;
		[Inject] private readonly ISettingsService _settingsService;
		[Inject] private readonly IEntityRepository _entityRepository;
		
		public override void CreateObjects()
		{
			var maxAiSpawnedCharacters = _settingsService.QualityPreset.SelectedPreset.Value.GetValue<int>(SettingsConsts.REMOVE_INITIAL_CHARACTERS, GameSetting.ParameterType.Int);
			_settingsService.QualityPreset.SelectedPreset.SkipLatestValueOnSubscribe().Subscribe(OnSettingsChanged).AddTo(this);
			
			for (var index = 0; index < SpawnPoints.Length && index < maxAiSpawnedCharacters; index++)
			{
				var spawnPoint = SpawnPoints[index];
				_entityAdapters.Add(spawnPoint.CreateAiAdapter(AdapterStrategy));
			}
		}
		
		public override UniTask CreateObjectsAsync()
		{
			CreateObjects();
			return UniTask.CompletedTask;
		}

		private void OnSettingsChanged(GameSetting setting)
		{
			var maxAiSpawnedCharacters = setting.GetValue<int>(SettingsConsts.REMOVE_INITIAL_CHARACTERS, GameSetting.ParameterType.Int);
			
			while(maxAiSpawnedCharacters < _entityAdapters.Count)
			{
				var entity = _entityAdapters[0];
				if(entity != null && entity.Entity != null) 
					entity.Entity.OnDestroyed(_entityRepository);
					
				if (entity is MonoBehaviour monoEntityAdapter)
				{
					if(monoEntityAdapter)
						Destroy(monoEntityAdapter.gameObject);
				}
				_entityAdapters.RemoveAt(0);
			}
			
			/*while(maxAiSpawnedCharacters > _entityAdapters.Count)
			{
				var point = SpawnPoints.GetRandom();
				_entityAdapters.Add(point.CreateAiAdapter(AdapterStrategy));
			}*/
		}

#if UNITY_EDITOR
		[Button]
		public void GetSpawnPoints()
		{
			SpawnPoints = transform.GetComponentsInChildren<AiSpawnPoint>();
		}
#endif
	}
}