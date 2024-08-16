using System;
using System.Collections.Generic;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using UnityEngine;

namespace Core.Entity.Step
{
	public class WaterSplashTrigger : MonoBehaviour, IInteractorVisiter
	{
		[SerializeField] private AudioClip _splashSound;

		private readonly List<Collider> _inTriggerObjects = new List<Collider>();
		
		private void OnTriggerEnter(Collider other)
		{
			if(_inTriggerObjects.Contains(other))
				return;

			PlaySplashSound(other.transform.position);
			_inTriggerObjects.Add(other);
		}

		private void OnTriggerExit(Collider other)
		{
			_inTriggerObjects.Remove(other);
		}

		private void PlaySplashSound(Vector3 position)
		{
			LucidAudio
				.PlaySE(_splashSound)
				.SetPosition(position)
				.SetVolume(0.3f)
				.SetSpatialBlend(1f);
		}
		public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage, ref InteractionCallMeta meta)
		{
			throw new NotImplementedException();
		}
		public InteractionResultMeta Accept(DragInteraction drag, ref InteractionCallMeta meta)
		{
			throw new NotImplementedException();
		}
		public InteractionResultMeta Accept(EnviromentProjectileInteraction environment, ref InteractionCallMeta meta)
		{
			throw new NotImplementedException();
		}
		public InteractionResultMeta Accept(EntityDamagable damagable, ref InteractionCallMeta meta)
		{
			throw new NotImplementedException();
		}
		public InteractionResultMeta Accept(EntityEffectable damagable, ref InteractionCallMeta meta)
		{
			throw new NotImplementedException();
		}
		public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta)
		{
			throw new NotImplementedException();
		}
		public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)
		{
			throw new NotImplementedException();
		}
	}
}
