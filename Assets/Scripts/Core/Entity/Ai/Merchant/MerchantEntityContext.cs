using Core.Entity.Ai.Npc;
using Core.HealthSystem;
using CustEditor.Attributes;
using Dialogue.Services.Interfaces;
using NodeCanvas.Framework;
using UniRx;
using UnityEngine;
using VContainer;

namespace Core.Entity.Ai.Merchant
{
	public class MerchantEntityContext : LifeEntity, IDialogueSource
	{
		[SerializeField] private CharacterHealth _health;
		[SerializeField] private Blackboard _blackboard;
		[SerializeField] private NpcAnimator _npcAnimator;
		[field:SerializeField, SpinnerDialogueNodeFinder] public string DialogueNodeName { get; set; }
		
		public Blackboard Blackboard => _blackboard;
		public NpcAnimator NpcAnimator => _npcAnimator;
		public override ILifeEntityHealth Health => _health;

		protected override void OnCreated(IObjectResolver resolver)
		{
			_health.Init();
			_health.AddTo(this);
			_health.SetContext(this);
			resolver.Inject(GetComponent<DialogueMonoInteraction>());
		}

		public string GetDialogueNodeName()
		{
			return DialogueNodeName;
		}
	}
}