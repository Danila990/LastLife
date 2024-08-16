using System.Linq;
using Core.Entity.Characters.Adapters;
using Core.Inventory;
using Core.Inventory.Items.Weapon;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Utils.Constants;

namespace Core.Entity.Ai.AiActions
{
	public class AiMeleeAction : ActionTask<AiCharacterAdapter>
	{
		[RequiredField]
		public BBParameter<float> AttackInterval;
		public BBParameter<IAiTarget> Target;
		public BBParameter<float> AffectiveDistance;
		
		private MeleeWeaponContext _melee;
		private SimpleTimerAction _action;

		protected override string OnInit()
		{
			_action = new SimpleTimerAction(AttackInterval.value);
			return null; 
		}

		protected override void OnExecute()
		{
			_melee ??= (MeleeWeaponContext)agent.CurrentContext.Inventory.InventoryItems.FirstOrDefault(context => context.ItemContext is MeleeWeaponContext).ItemContext;
			if (!_melee)
			{
				EndAction(false);
				return;
			}

			_action.CanUse(true);
			
			_action.SetAction(RandomAttack);
		}
		
		private void RandomAttack()
		{
			if(!_melee)
				return;
			agent.CurrentContext.CharacterAnimator.Animator.SetBool(AHash.IsMeleeAttack, false);
			if (Random.Range(0, 100) > 50)
				_melee.Attack();
			else
				_melee.KickTask().Forget();
		}
		
		protected override void OnUpdate()
		{
			var valueLookAtPoint = Target.value.LookAtPoint;
			var delta = (valueLookAtPoint - agent.transform.position).normalized;
			delta.y = 0;
			agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, Quaternion.LookRotation(delta), Time.deltaTime * 4);

			var deltaTime = Time.deltaTime;
			_action.Tick(ref deltaTime);
			
			if ((Target.value.MovePoint - agent.transform.position).sqrMagnitude < AffectiveDistance.value * AffectiveDistance.value)
				_action.CanUse(true);
			else
				_action.CanUse(false);
				
		}


		protected override void OnStop()
		{
			_action.CanUse(false);
			agent.CurrentContext.CharacterAnimator.Animator.SetBool(AHash.IsMeleeAttack, false);
		}

	}

}
