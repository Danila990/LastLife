using Core.CameraSystem;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.Factory;
using Core.HealthSystem;
using Core.Services;
using Cysharp.Threading.Tasks;
using Db.ObjectData;
using MessagePipe;
using SharedUtils;
using UnityEngine;
using VContainer.Unity;

namespace Core.InputSystem
{
    public interface IPlayerSpawnService
    {
        PlayerCharacterAdapter PlayerCharacterAdapter { get; }
        CharacterObjectData ActiveCharacterData { get; }
        void CreatePlayerContext(CharacterObjectData characterData);
        void CreatePlayerFromId(string id);
        void CreatePlayerContextAt(CharacterObjectData characterData, Transform point);
        void ManualDestroyCharacter();
    }
    
    public class PlayerSpawnService : IPostInitializable, IPlayerSpawnService
    {
        private readonly IPlayerCharacterFactory _characterFactory;
        private readonly ICameraService _cameraService;
        private readonly IObjectFactory _objectFactory;
        private readonly IEntityRepository _entityRepository;
        private readonly ISpawnPointProvider _spawnPointProvider;
        private readonly IPublisher<PlayerContextChangedMessage> _publisher;
        private readonly IItemStorage _itemStorage;

        public PlayerCharacterAdapter PlayerCharacterAdapter { get; private set; }
        public CharacterObjectData ActiveCharacterData { get; private set; }


        public PlayerSpawnService(
            IPlayerCharacterFactory characterFactory,
            ICameraService cameraService,
            IObjectFactory objectFactory, 
            IEntityRepository entityRepository,
            ISpawnPointProvider spawnPointProvider,
            IPublisher<PlayerContextChangedMessage> publisher,
            IItemStorage itemStorage)
        {
            _characterFactory = characterFactory;
            _cameraService = cameraService;
            _objectFactory = objectFactory;
            _entityRepository = entityRepository;
            _spawnPointProvider = spawnPointProvider;
            _publisher = publisher;
            _itemStorage = itemStorage;
        }
		
        public void PostInitialize()
        {
            var point = _spawnPointProvider.GetSafeSpawnPoint();
            var rotation = point.rotation;
            var position = point.position;
            
            PlayerCharacterAdapter = _characterFactory.CreatePlayerAdapterOnly(position, rotation);
            _cameraService.CurrentBrain.transform.parent.SetPositionAndRotation(position, rotation);
            _cameraService.SetTrackedAdapter(PlayerCharacterAdapter);
        }
        
        public void CreatePlayerContext(CharacterObjectData characterData)
        {
            var point = _spawnPointProvider.GetSafeSpawnPoint();
            CreatePlayerContextAt(characterData, point);
        }
        
        public void CreatePlayerFromId(string id)
        {
            CreatePlayerContext(_itemStorage.Characters[id]);
        }

        public void CreatePlayerContextAt(CharacterObjectData characterData, Transform point)
        {
            ActiveCharacterData = characterData;
            CreatePlayerContextAtInternal(characterData.PlayerId, point);
        }

        private void CreatePlayerContextAtInternal(string contextId, Transform point)
        {
            if (PlayerCharacterAdapter.CurrentContext)
            {
                if (PlayerCharacterAdapter.CurrentContext.Health.IsDeath)
                {
                    PlayerCharacterAdapter.transform.SetPositionAndRotation(point.position, point.rotation);
                }
                else if (!_spawnPointProvider.InBoundsMap(PlayerCharacterAdapter.transform.position))
                {
                    PlayerCharacterAdapter.transform.SetPositionAndRotation(point.position, point.rotation);
                }
                _publisher.Publish(new PlayerContextChangedMessage(false, PlayerCharacterAdapter.CurrentContext));
                PlayerCharacterAdapter.CurrentContext.OnDestroyed(_entityRepository);
            }
            else
            {
                PlayerCharacterAdapter.transform.SetPositionAndRotation(point.position, point.rotation);
            }
            var context = _objectFactory.CreateObject(contextId) as CharacterContext;
            TempImmortal(context).Forget();
            
            _entityRepository.AddGenericEntity(context);
            PlayerCharacterAdapter.SetContextId(contextId);
            PlayerCharacterAdapter.SetEntityContext(context);
            _publisher.Publish(new PlayerContextChangedMessage(true, context));
        }

        private async static UniTaskVoid TempImmortal(CharacterContext characterContext)
        {
            characterContext.SetImmortal(true);
            await UniTask.Delay(1f.ToSec(), cancellationToken:characterContext.destroyCancellationToken);
            characterContext.SetImmortal(false);
        }

        public void ManualDestroyCharacter()
        {
            _publisher.Publish(new PlayerContextChangedMessage(false, PlayerCharacterAdapter.CurrentContext));
            PlayerCharacterAdapter.CurrentContext.OnDestroyed(_entityRepository);
        }
    }

    public readonly struct PlayerContextChangedMessage
    {
        public readonly bool Created;
        public readonly CharacterContext CharacterContext;
        
        public PlayerContextChangedMessage(bool created, CharacterContext characterContext)
        {
            Created = created;
            CharacterContext = characterContext;
        }
    }

    public readonly struct PlayerContextDeathMessage
    {
        public readonly DiedArgs DiedArgs;

        public PlayerContextDeathMessage(DiedArgs diedArgs)
        {
            DiedArgs = diedArgs;
        }
    }
    
    public readonly struct PlayerContextDamageMessage
    {
        public readonly DamageArgs DamageArgs;
        
        public PlayerContextDamageMessage(DamageArgs damageArgs)
        {
            DamageArgs = damageArgs;
        }
    }
}