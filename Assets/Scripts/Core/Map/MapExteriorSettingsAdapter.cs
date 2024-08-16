using System.Collections.Generic;
using Core.Entity;
using Core.Entity.Repository;
using Cysharp.Threading.Tasks;
using GameSettings;
using UniRx;
using Utils.Constants;
using VContainer;

namespace Core.Map
{
	public class MapExteriorSettingsAdapter : MapExterior
	{
		[Inject] private readonly ISettingsService _settingsService;
		[Inject] private readonly IEntityRepository _entityRepository;

		private List<EntityContext> _entityContexts = new List<EntityContext>();
		
		public override void CreateObjects()
		{
			var isRemoveExterior = GetSettings();

			if (isRemoveExterior)
				return;

			foreach (var spawnPoint in SpawnPoints)
			{
				_entityContexts.Add(spawnPoint.CreateObject(ObjectFactory, false));
			}
		}

		public async override UniTask CreateObjectsAsync()
		{
			var isRemoveExterior = GetSettings();
			if (isRemoveExterior)
				return;
			
			for (var index = 0; index < SpawnPoints.Length; index++)
			{
				var spawnPoint = SpawnPoints[index];
				var entity = spawnPoint.CreateObject(ObjectFactory, false);
				_entityContexts.Add(entity);
				if (index % 5 == 0)
				{
					await UniTask.Yield(destroyCancellationToken);
				}
			}
		}
		
		private bool GetSettings()
		{
			var isRemoveExterior = _settingsService.QualityPreset.SelectedPreset.Value.GetValue<bool>(SettingsConsts.REMOVE_MAP_EXTERIOR, GameSetting.ParameterType.Bool);
			_settingsService.QualityPreset.SelectedPreset.SkipLatestValueOnSubscribe().Subscribe(OnSettingsChanged).AddTo(this);
			return isRemoveExterior;
		}

		private void OnSettingsChanged(GameSetting setting)
		{
			var isRemoveExterior = setting.GetValue<bool>(SettingsConsts.REMOVE_MAP_EXTERIOR, GameSetting.ParameterType.Bool);
			if (isRemoveExterior)
			{
				foreach (var entity in _entityContexts)
				{
					if(entity == null)
						continue;
					
					entity.OnDestroyed(_entityRepository);
					Destroy(entity.gameObject);
				}
				_entityContexts = new List<EntityContext>();
			}
			else
			{
				if (_entityContexts.Count > 0)
					return;
				
				foreach (var spawnPoint in SpawnPoints)
				{
					_entityContexts.Add(spawnPoint.CreateObject(ObjectFactory, false));
				}
			}
		}
	}
}