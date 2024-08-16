using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Repository;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Equipment.Impl.Hat
{
	public class HaloEquipmentEntity : HatEquipmentEntity
	{
		[SerializeField, TitleGroup("Params")] private float _resurrectPercent;
		[SerializeField, TitleGroup("Params")] private float _immortalDuration;
		[SerializeField, BoxGroup("Visual")] private Renderer _renderer;
		[SerializeField, BoxGroup("Visual")] private ParticleSystem _fx; 
		[SerializeField, BoxGroup("Visual"), ColorUsage(false, true)] private Color _color;
		[SerializeField, BoxGroup("Sound")] private AudioClip _clip;
		[SerializeField, BoxGroup("Sound")] private float _volume;
		
		private Health _health;
		private IDisposable _disposable;

		protected override void OnCreated(IObjectResolver resolver)
		{
			base.OnCreated(resolver);
			
			_health = new Health();
			_health.Init();
			_disposable = _health.OnDeath.Subscribe(OnDeath);
		}

		protected override void OnPutOnInternal()
		{
			base.OnPutOnInternal();
			if (Owner == null)
				return;
			
			Owner.Health.SetProxyHealth(_health);
		}

		protected override void OnTakeOffInternal()
		{
			base.OnTakeOffInternal();
			if (Owner == null)
				return;
			
			Owner.Health.RemoveProxyHealth(_health);
		}

		private void OnDeath(DiedArgs args)
		{
			_disposable?.Dispose();
			_fx.Play();
			PlaySound();
			_renderer.material.SetColor(ShHash.EmissionColor, _color);
			_renderer.material.SetColor(ShHash.BaseColor, _color);
			ImmortalAsync().Forget();
			BlockInInventory();

		}

		private void BlockInInventory()
		{
			if (Inventory.TryGetController(out var controller))
			{
				var equipmentItem = controller.AllEquipment.EquipmentByTypeId[(PartType, CurrentItemArgs.Id)];
				equipmentItem.IsBlocked = true;
				controller.AllEquipment.ChangePart(equipmentItem);
			}
		}
		
		private async UniTaskVoid ImmortalAsync()
		{
			Owner.SetImmortal(true);
			await UniTask.Delay(_immortalDuration.ToSec(), cancellationToken: destroyCancellationToken);
			Owner.SetImmortal(false);
			Owner.Health.Resurrect(_resurrectPercent);

		}
		
		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			_disposable?.Dispose();
			_health.Dispose();
		}
		
		private void PlaySound()
		{
			LucidAudio
				.PlaySE(_clip)
				.SetPosition(MainTransform.position)
				.SetSpatialBlend(1f)
				.SetVolume(_volume);
		}
	}
}
