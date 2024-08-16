using Core.CameraSystem;
using Core.Entity.Characters.Adapters;
using Core.Factory.DataObjects;
using UnityEngine;
using VContainer;

namespace Core.Factory
{
    public class PlayerCharacterFactory : IPlayerCharacterFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly IPlayerFactoryData _factoryData;
        private readonly ICameraService _cameraService;

        public PlayerCharacterFactory(
            IObjectResolver resolver,
            IPlayerFactoryData factoryData,
            ICameraService cameraService
        )
        {
            _resolver = resolver;
            _factoryData = factoryData;
            _cameraService = cameraService;
        }
        
        public PlayerCharacterAdapter CreatePlayerAdapterOnly(Vector3 pos = default, Quaternion spawnPointRotation = default)
        {
            var adapter = Object.Instantiate(_factoryData.Adapter, pos, spawnPointRotation);
            _resolver.Inject(adapter);
            
            adapter.BindMainCameraTransform(_cameraService.CurrentBrain.transform);
            adapter.Init();
            
            return adapter;
        }
    }
}