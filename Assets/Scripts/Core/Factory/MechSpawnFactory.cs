using System.Linq;
using System.Threading;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters;
using Core.InputSystem;
using Core.Inventory;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Core.Factory
{
    public class MechSpawnFactory : IAsyncStartable
    {
        private readonly IMechAdapterFactory _mechAdapterFactory;
        private readonly IObjectFactory _objectFactory;
        private readonly ICameraService _cameraService;
        private readonly IPlayerSpawnService _playerSpawnService;
        private readonly IPlayerInputProvider _inputProvider;
        private readonly IInventoryService _inventoryService;

        public MechSpawnFactory(
            IMechAdapterFactory mechAdapterFactory,
            IObjectFactory objectFactory,
            ICameraService cameraService,
            IPlayerSpawnService playerSpawnService,
            IPlayerInputProvider inputProvider,
            IInventoryService inventoryService
        )
        {
            _mechAdapterFactory = mechAdapterFactory;
            _objectFactory = objectFactory;
            _cameraService = cameraService;
            _playerSpawnService = playerSpawnService;
            _inputProvider = inputProvider;
            _inventoryService = inventoryService;
        }
        
        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await UniTask.Delay(5000, cancellationToken: cancellation);
            return;
            var adapter = _mechAdapterFactory.CreateMechAdapter();
            var context = _objectFactory.CreateObject("TriMech") as MechEntityContext;
            adapter.SetMechContext(context);
        }
    }
}