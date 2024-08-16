using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Head.Bird;
using Core.Factory;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using NodeCanvas.Framework;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Ai.AiItem
{
	public class SpawnEggMonoItem : MonoAiItem
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.AllIds"),
		 InlineButton("@Core.Factory.DataObjects.FactoryData.EditorInstance.UpdateValues()", SdfIconType.Circle, ""),
		 SerializeField]
		 private string _entityToSpawnId;
		[SerializeField] private Blackboard _blackboard;
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private int _maxActiveCount;
		[SerializeField] private AudioClip _openMouth;
		private int _activeCount;

		[Inject] private readonly IObjectFactory _objectFactory;

		private HeadBird BirdOwner { get; set; }
		private Variable<bool> _interrupted;
		private float _timer;
		public override void Created(EntityContext owner)
		{
			base.Created(owner);
			BirdOwner = (HeadBird)owner;
			_interrupted = _blackboard.GetVariable<bool>("Interrupted");
		}
		
		protected override void OnUse(IAiTarget aiTarget)
		{
			AsyncCreate().Forget();
		}

		private void PlayOpenMouth()
		{
			if(!_openMouth)
				return;

			LucidAudio
				.PlaySE(_openMouth)
				.SetPosition(BirdOwner.MainTransform.position)
				.SetVolume(1f)
				.SetSpatialBlend(1f);
		}
		
		private async UniTaskVoid AsyncCreate()
		{
			BirdOwner.Animator.SetTrigger(AHash.MouseOpen);
			PlayOpenMouth();
			BirdOwner.HeartDamagable.gameObject.SetActive(true);
			
			_timer = 0;
			await UniTask.WaitWhile(WaitWhileNotInterrupted, cancellationToken: destroyCancellationToken);

			if (_interrupted.value)
				return;

			SpawnEgg();

			_timer = 0;
			await UniTask.WaitWhile(WaitWhileNotInterrupted, cancellationToken: destroyCancellationToken);
			EndUse(true);
		}
		
		[Button]
		private void SpawnEgg()
		{
			NavMesh.SamplePosition(_spawnPoint.position, out var hit, 3, NavMesh.AllAreas);
			var context = (BirdEggEntity)_objectFactory.CreateObject(_entityToSpawnId, hit.position, _spawnPoint.rotation);
			context.OnSpawnOrDestroyChild.TakeUntilDestroy(context).Subscribe(OnDeadOrSpawnedBird);
			
			_activeCount++;
			_blackboard.SetVariableValue("CanSpawn", _activeCount < _maxActiveCount);
		}

		private bool WaitWhileNotInterrupted()
		{
			if (_interrupted.value)
			{
				EndUse(false);
				return false;
			}
			else
			{
				_timer += Time.deltaTime;
				if (_timer >= 5)
				{
					return false;
				}
			}
			return true;
		}

		private void OnDeadOrSpawnedBird(IObservable<DiedArgs> obj)
		{
			if(obj is null)
			{
				_activeCount--;
				_blackboard.SetVariableValue("CanSpawn", _activeCount < _maxActiveCount);
			}
			else
			{
				obj.TakeUntilDestroy(this).Subscribe(OnSpawnedDeath);
			}
		}

		private void OnSpawnedDeath(DiedArgs obj)
		{
			_activeCount--;
			_blackboard.SetVariableValue("CanSpawn", _activeCount < _maxActiveCount);
		}

		protected override void OnEnd(bool sucsess)
		{
			BirdOwner.HeartDamagable.gameObject.SetActive(false);
		}
		
		public override void Tick(ref float deltaTime)
		{
		}
		
		public override void OnAnimEvent(object args)
		{
		}
		public override void Dispose()
		{
			
		}
	}
}