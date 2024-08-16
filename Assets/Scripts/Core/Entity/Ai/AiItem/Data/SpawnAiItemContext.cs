using System;
using System.Threading;
using Core.Entity.Head;
using Core.Inventory.Items.Weapon;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.Entity.Ai.AiItem.Data
{
	public class SpawnAiItemContext : AiItemContextDecorator, ISpawnEntityListener
	{
		private readonly SpawnLifeEntityWeaponContext _context;
		private readonly JetHeadContext _jetHead;

		private int _aliveBombs;
		private CompositeDisposable _disposable;

		public SpawnAiItemContext(SpawnLifeEntityWeaponContext context, EntityContext owner, AiItemContextedData aiItemContextedData) : base(context, owner, aiItemContextedData)
		{
			_context = context;
			_jetHead = (JetHeadContext)owner;
		}
		
		protected override void OnUse(IAiTarget aiTarget)
		{
			DropBomb( _jetHead.destroyCancellationToken).Forget();
		}

		private async UniTaskVoid DropBomb(CancellationToken token)
		{
			await _jetHead.OpenDoors();
			await UniTask.Delay(0.2f.ToSec(), cancellationToken:token);
			
			_disposable?.Dispose();
			_disposable = new CompositeDisposable();
			_context.Activate(this);
			
			await UniTask.Delay(0.5f.ToSec(), cancellationToken:token);
			
			await _jetHead.CloseDoors();
			await UniTask.Delay(0.2f.ToSec(), cancellationToken:token);

			EndUse(true);
		}
		
		protected override void OnEnd(bool sucsess)
		{
			
		}

		public override float GetPriority(IAiTarget aiTarget)
		{
			if (_aliveBombs > 0)
				return -100f;
			
			return base.GetPriority(aiTarget);
		}

		public void OnEntitySpawned(EntityContext context)
		{
			if (context is JetBombContext jetBombContext)
			{
				AddBomb(jetBombContext);
				jetBombContext.SetAiTarget(AiTarget);
			}
		}

		private void AddBomb(JetBombContext context)
		{
			_aliveBombs++;
			context.Health.OnDeath.Subscribe(_ => OnBombDied()).AddTo(_disposable);
		}

		private void OnBombDied()
		{
			_aliveBombs--;
			
			if (_aliveBombs == 0)
			{
				_disposable?.Dispose();
				_disposable = null;
			}
		}
	}
}