using System;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.InputSystem;
using MessagePipe;
using UniRx;

namespace Core.Services
{
	public interface IDeleteItemService
	{
		void RayCastDestroy();
		public ReactiveCommand OnObjectDestroy { get; }
		public IReactiveProperty<bool> DeleteStatus { get; }
	} 
	
	public class DeleteItemService : IInteractorVisiter, IDeleteItemService, IDisposable
	{
		private readonly IEntityRepository _entityRepository;
		private readonly IRayCastService _rayCastService;		
		private readonly ReactiveCommand _onObjectDestroy = new();
		private readonly BoolReactiveProperty _deleteStatus = new();
		private uint _characterContextUid;
		private readonly IDisposable _disposable;
		
		public ReactiveCommand OnObjectDestroy => _onObjectDestroy;
		public IReactiveProperty<bool> DeleteStatus => _deleteStatus;

		public DeleteItemService(
			IEntityRepository entityRepository,
			IRayCastService rayCastService,
			ISubscriber<PlayerContextChangedMessage> subscriber
			)
		{
			_entityRepository = entityRepository;
			_rayCastService = rayCastService;
			_disposable = subscriber.Subscribe(message =>
			{
				_characterContextUid = message.Created ? message.CharacterContext.Uid : (uint)0;
			});
		}

		public void RayCastDestroy()
		{
			_rayCastService.SphereCastRayInteract(0.15f,true, this,100, _characterContextUid);
		}
		
		public InteractionResultMeta Accept(EntityDestroyInteractable destroyInteractable, ref InteractionCallMeta meta)
		{
			destroyInteractable.Destroy(_entityRepository);
			_onObjectDestroy.Execute();
			return StaticInteractionResultMeta.InteractedBlocked;
		}

		public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)=> StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
		public InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;


		public void Dispose()
		{
			_onObjectDestroy?.Dispose();
			_deleteStatus?.Dispose();
			_disposable?.Dispose();
		}
	}
}