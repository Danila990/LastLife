using System.Threading;
using AnnulusGames.LucidTools.Audio;
using Core.AnimationRigging;
using Core.CameraSystem;
using Core.Entity.Ai.AiItem.Data;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Entity.Repository;
using Core.InputSystem;
using Cysharp.Threading.Tasks;
using LitMotion;
using RootMotion.Dynamics;
using SharedUtils;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Ai.AiItem
{

	public class TongueAiItem : AbstractAiItem, IInteractorVisiter, IDestroyListener
	{
		public override float UseActionDuration => _aiItemData.HandleForwardDuration + _aiItemData.HandleBackDuration;
		private readonly HeadContext _owner;
		private readonly TongueAiItemData _aiItemData;

		private DragInteraction _currentInteraction;
		private RigElementController _rig;
		private MotionHandle _handle;
		private Vector3 _rigTargetLocalPosition;
		private LifeEntity _lifeEntity;

		[Inject] private readonly ICameraService _camera;
		[Inject] private readonly IEntityRepository _repository;
		[Inject] private readonly IPlayerSpawnService _playerSpawnService;

		public TongueAiItem(HeadContext owner, TongueAiItemData aiItemData) : base(owner, aiItemData)
		{
			_aiItemData = aiItemData;
			_owner = owner;
		}
		public override float GetPriority(IAiTarget aiTarget)
		{
			if (aiTarget.IsImmortal)
				return -10f;
			return base.GetPriority(aiTarget);
		}

		protected override float GetDistancePriority(IAiTarget aiTarget)
		{
			var distance = GetDistanceToTarget(aiTarget);
			if (distance < UseRange)
			{
				return -distance - UseRange;
			}
			return distance * 1.5f;
		}

		public override void Created()
		{
			base.Created();
			_rig = _owner.RigProvider.Rigs[_aiItemData.RigName];
			_rigTargetLocalPosition = _rig.RigTarget.localPosition;
		}

		protected override void OnUse(IAiTarget aiTarget)
		{
			if (!aiTarget.TryGetEntity(out _lifeEntity))
				return;
			
			if (aiTarget.IsImmortal)
			{
				EndUse(true);
				return;
			}

			if (Physics.Linecast(_owner.LookAtTransform.position, _lifeEntity.LookAtTransform.position, LayerMasks.Environment))
			{
				EndUse(true);
			}
			else
			{
				_owner.Animator.SetBool(AHash.MouseOpen, true);
				_rig.EnableRig();
				MoveTongueToTarget(_owner.destroyCancellationToken).Forget();
			}
		}

		private async UniTaskVoid MoveTongueToTarget(CancellationToken token)
		{
			var duration = _aiItemData.HandleForwardDuration;
			var connectToTarget = false;
			while (
				!token.IsCancellationRequested &&
				InUse &&
				AiTarget.IsActive &&
				duration > 0)
			{
				_rig.RigTarget.position = Vector3.MoveTowards(_rig.RigTarget.position, AiTarget.LookAtPoint, Time.deltaTime * _aiItemData.ForwardSpeed);
				var distance = Vector3.Distance(_rig.RigTarget.position, AiTarget.LookAtPoint);
				if (distance < _aiItemData.DistanceThreshold)
				{
					connectToTarget = true;
					break;
				}
				if (distance > UseRange * 2.5f)
				{
					break;
				}
				
				duration -= Time.deltaTime;
				await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken: token);
			}

			if (connectToTarget)
			{
				CreateConnect();
				await UniTask.Delay(0.25f.ToSec(), cancellationToken: token);
			}
			
			await MoveTongueToMouse(token);
		}
		
		private void CreateConnect()
		{
			var meta = new InteractionCallMeta
			{
				OriginPoint = Owner.transform.position,
				Point = _rig.RigTarget.position, 
				Normal = _rig.RigTarget.forward, 
				Distance = Vector3.Distance(_rig.RigTarget.position, AiTarget.LookAtPoint)
			};
			
			var result = AiTarget.Visit(this, ref meta);
			if (result.Interacted && _lifeEntity is CharacterContext context)
			{
				context.PuppetMaster.state = PuppetMaster.State.Dead;
				context.PuppetMaster.muscles.ForEach(muscle => muscle.rigidbody.detectCollisions = false);
				if (context.CurrentAdapter is AiCharacterAdapter adapter)
				{
					adapter.GetComponent<NavMeshAgent>().enabled = false;
					adapter.SetBinded(false);
				}
				else
				{
					_camera.SetTrackedTarget(_owner);
				}
			}
		}

		private async UniTask MoveTongueToMouse(CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				EndUse(true);
				return;
			}
			
			await LMotion
				.Create(
					_rig.RigTarget.localPosition,
					_rigTargetLocalPosition, 
					Mathf.Min(_aiItemData.HandleBackDuration / UseRange * Vector3.Distance(_rig.RigTarget.localPosition, _rigTargetLocalPosition), _aiItemData.HandleBackDuration))
				.Bind(MoveBack)
				.ToUniTask(cancellationToken: token);
			
			_owner.Animator.SetBool(AHash.MouseOpen, false);
#pragma warning disable CS4014
			_rig.DisableRig();
			
#region OnToungueInHead

			if (_currentInteraction)
			{
				_owner.Animator.SetBool(AHash.Eating, true);
				await UniTask.Delay(0.15f.ToSec(), cancellationToken: token);
				var animDuration = 3.25f;
				if (_owner.VFXFactory.TryGetParticle(_aiItemData.MeatFXName, out var particle))
				{
					particle.transform.SetParent(_owner.transform);
					particle.transform.forward = _owner.transform.forward;
					particle.transform.localPosition = Vector3.forward * 3f; 
					particle.Play();
				}

				if (_aiItemData.StartEat)
				{
					LucidAudio.PlaySE(_aiItemData.StartEat).SetPosition(_owner.LookAtTransform.position).SetSpatialBlend(1f);
				}
				await UniTask.WaitWhile(() =>
				{
					var deltaTime = Time.deltaTime;
					animDuration -= deltaTime;
					var targetRotation = Quaternion.identity;
					var pos = GetTargetPos();
					if (particle)
					{
						particle.transform.position = pos;
					}
					if(_currentInteraction != null)
						_currentInteraction.Drag(ref pos, ref targetRotation, ref deltaTime);
					
					return animDuration > 0;
				}, cancellationToken: _owner.destroyCancellationToken);
				
				if (particle)
				{
					_owner.VFXFactory.Release(_aiItemData.MeatFXName, particle);
				}

				_owner.Animator.SetBool(AHash.Eating, false);

				
				if (_currentInteraction)
				{
					_currentInteraction.OnEndDrag();
				}
				
				if (_lifeEntity)
				{
					_lifeEntity.Health.ForceDeath();
					if (_lifeEntity is CharacterContext {Adapter: PlayerCharacterAdapter } context)
					{
						_playerSpawnService.ManualDestroyCharacter();
					}
					else
					{
						_lifeEntity.OnDestroyed(_repository);
						Object.Destroy(_lifeEntity.gameObject);
					}
					
					_owner.Health.AddHealthPercent(0.1f);
				}
				
				if (_aiItemData.EndEat)
				{
					LucidAudio.PlaySE(_aiItemData.EndEat).SetPosition(_owner.LookAtTransform.position).SetSpatialBlend(1);
				}
				
				_currentInteraction = null;
				_lifeEntity = null;
			}
#endregion

			EndUse(true);
		}
		
		private void MoveBack(Vector3 obj)
		{
			_rig.RigTarget.localPosition = obj;
			if(!InUse) return;
			if (_currentInteraction is null)
			{
				return;
			}
			var targetRotation = Quaternion.FromToRotation(_currentInteraction.transform.up, Vector3.up) * _currentInteraction.transform.rotation;
			var fixedDeltaTime = Time.deltaTime;
			var pos = GetTargetPos();
			_currentInteraction.Drag(ref pos, ref targetRotation, ref fixedDeltaTime);
		}
		

		protected override void OnEnd(bool success)
		{

		}
		
		public void Release()
		{
			_currentInteraction = null;
			_lifeEntity = null;
			//MoveTongueToMouse(_owner.destroyCancellationToken).Forget();
		}
		
		private Vector3 GetTargetPos()
		{
			return _rig.RigTarget.position;
		}

		public override void Dispose()
		{
			if (_handle.IsActive())
			{
				_handle.Cancel();
			}
		}

		public InteractionResultMeta Accept(DragInteraction interaction, ref InteractionCallMeta meta)
		{
			interaction.AttachInteractor(this);
			_currentInteraction = interaction;
			//SetCurrDist(meta.Distance + _rayCastService.PlaneDist);
			interaction.OnBeginDrag(GetTargetPos());
			return StaticInteractionResultMeta.InteractedBlocked;
		}

		public InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;

		public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta) => StaticInteractionResultMeta.InteractedBlocked;
		public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.InteractedBlocked;
		public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta) => StaticInteractionResultMeta.InteractedBlocked;
		public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.InteractedBlocked;
		public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.InteractedBlocked;
	}
}