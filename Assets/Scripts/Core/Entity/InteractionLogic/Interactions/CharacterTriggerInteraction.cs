using System;
using Core.Entity.Characters.Adapters;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class CharacterTriggerInteraction : TriggerInteraction
	{
		[field:SerializeField] public PlayerCharacterAdapter Adapter { get; private set; }

		public override uint Uid => 9999;
	}

}
