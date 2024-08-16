using Core.Entity.Characters;
using Core.Entity.Head;
using Core.Quests.Messages;
using Core.Services;
using Cysharp.Threading.Tasks;
using Installer;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class SkipBossAction : MonoBehaviour, IInjectableTag
	{
		public GenericInteraction GenericInteraction;
		private IBossSpawnService _bossSpawnService;
		private IQuestMessageSender _questMessageSender;
		
		[Inject]
		private void Construct(IBossSpawnService bossSpawnService, IQuestMessageSender questMessageSender)
		{
			_bossSpawnService = bossSpawnService;
			_questMessageSender = questMessageSender;
			var property = bossSpawnService.CurrentBoss;
			bossSpawnService.CurrentBoss.Subscribe(_ => SetDontShow(property.Value)).AddTo(destroyCancellationToken);
			bossSpawnService.HasBossInRoom.Subscribe(_ => SetDontShow(property.Value)).AddTo(destroyCancellationToken);
		}
		private void SetDontShow(HeadContext context)
		{
			GenericInteraction.DontShow = context is not null || !_bossSpawnService.HasBossInRoom.Value;
		}

		private void Start()
		{
			GenericInteraction.Used.Subscribe(SkipBossTimer).AddTo(destroyCancellationToken);
		}
		
		private void SkipBossTimer(CharacterContext obj)
		{
			GenericInteraction.DontShow = true;
			_bossSpawnService?.SetTimerTime(0);
			_questMessageSender.SendSkipBossMessage();
		}
	}
}