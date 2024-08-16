using System.Threading;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.InteractionLogic;
using Core.Factory.DataObjects;
using Core.Services;
using Cysharp.Threading.Tasks;
using Ui.Sandbox.SpawnMenu;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Inventory.Items
{
	public class GravyGun : ItemContext
	{
		[Inject] private readonly IDragInteractionService _dragInteraction;
		[Inject] private readonly IDrawLineData _drawLineData;
		[Inject] private readonly IRayCastService _rayCastService;
		[Inject] private readonly ISpawnItemService _spawnItemService;
		[Inject] private readonly IDeleteItemService _deleteItemService;
		
		public AudioClip InteractSound;
		public AudioClip SpawnSound; 
		public AudioClip RemoveSound;
		private bool _lineStatus;
		private AudioPlayer _audioPlayer;
		private bool _cancelShootLine;
		private bool _inited;
		private LineDrawController _lineDrawController;

		protected override void OnCreated(IObjectResolver resolver)
		{
			base.OnCreated(resolver);
			_lineDrawController = new LineDrawController(_drawLineData);
			_lineDrawController.Init();
		}

		public override void ItemInit(IOriginProxy inventory)
		{
			base.ItemInit(inventory);
			if(!IsPlayerOwned || _inited)
				return;
			
			_inited = true;
			_dragInteraction.InputStatus.SkipLatestValueOnSubscribe().Subscribe(OnInputStatusChange).AddTo(this);
			_spawnItemService.ObjectSpawn.Subscribe(OnObjectSpawn).AddTo(this);
			_deleteItemService.OnObjectDestroy.Subscribe(OnObjectDestroy).AddTo(this);
			_dragInteraction.DragInteractor.DragStatus.Subscribe(DragChange).AddTo(this);
		}

		private void ShootLineOnce()
		{
			_cancelShootLine = false;
			_lineStatus = true;
			DrawShootLine(destroyCancellationToken).Forget();
		}
		
		private void OnInputStatusChange(bool state)
		{
			if (_audioPlayer is not null)
			{
				_audioPlayer.Stop();
				_audioPlayer = null;
			}
					
			_audioPlayer = LucidAudio
				.PlaySE(InteractSound)
				.SetSpatialBlend(1f)
				.SetLoop(true)
				.SetVolume(state ? 0.25f : 0f)
				.SetPosition(ItemAnimator.RuntimeModel.Origin.position);
		}
		
		private void DragChange(bool status)
		{
			_lineDrawController.DisableLine();
			_lineStatus = status;
		}

		private void LateUpdate()
		{
			if(!IsPlayerOwned || !ItemAnimator.RuntimeModel) 
				return;

			if (_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop && ItemAnimator.RuntimeModel)
			{
				_audioPlayer.SetPosition(ItemAnimator.RuntimeModel.Origin.position);
			}
			
			if (_dragInteraction.TryInteract || _deleteItemService.DeleteStatus.Value)
			{
				ShootLine();
				_cancelShootLine = true;
				return;
			}

			if (!_lineStatus)
			{
				_lineDrawController.DisableLine();
				return;
			}
			
			if(!_dragInteraction.DragInteractor.InDrag) 
				return;
			
			BezierLine();
			_cancelShootLine = true;
		}
		
		private void BezierLine()
		{
			if(!_dragInteraction.DragInteractor.CurrentInteraction) 
				return;
			
			_lineDrawController.DrawLine(
				ItemAnimator.RuntimeModel.Origin.position,
				_dragInteraction.DragInteractor.TargetPos,
				_dragInteraction.DragInteractor.CurrentInteraction.TargetRigidbody.position
			);
		}

		public override void OnDeselect()
		{
			base.OnDeselect();
			_lineDrawController.DisableLine();
		}

		private void ShootLine()
		{
			_lineDrawController.DrawLine(
				ItemAnimator.RuntimeModel.Origin.position,
				Vector3.Lerp(ItemAnimator.RuntimeModel.Origin.position,_rayCastService.CurrentHitPoint,0.5f),
				_rayCastService.CurrentHitPoint
			);
		}
		
		private void OnObjectDestroy(Unit _)
		{
			LucidAudio
				.PlaySE(RemoveSound).
				SetPosition(ItemAnimator.RuntimeModel.Origin.position)
				.SetVolume(0.075f);
		}
		
		private void OnObjectSpawn(Unit _)
		{
			LucidAudio.PlaySE(SpawnSound)
				.SetPosition(ItemAnimator.RuntimeModel.Origin.position)
				.SetVolume(0.075f);
			ShootLineOnce();
		}

		private async UniTaskVoid DrawShootLine(CancellationToken token)
		{
			if(_dragInteraction.DragInteractor.InDrag)
				return;
			
			for (var i = 0; i < 10; i++)
			{
				if (_cancelShootLine)
				{
					_cancelShootLine = false;
					_lineStatus = false;
					_lineDrawController.DisableLine();
					return;
				}
				ShootLine();
			}
			await UniTask.NextFrame(token);
			_lineDrawController.DisableLine();
			_lineStatus = false;
		}
	}
}