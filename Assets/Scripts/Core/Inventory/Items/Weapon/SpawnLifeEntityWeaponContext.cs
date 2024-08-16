using Core.Entity;
using Core.Factory;
using Core.Inventory.Origins;
using UnityEngine;
using VContainer;

namespace Core.Inventory.Items.Weapon
{
	public interface ISpawnEntityListener
	{
		void OnEntitySpawned(EntityContext context);
	}
	
	public class SpawnLifeEntityWeaponContext : WeaponContext
	{
		public string SpawnId;
		public string OriginId;
		public float Cooldown;

		public int SpawnCount;
		public Vector3 SpawnForce;
		
		[Inject] private readonly IObjectFactory _objectFactory;
		public BaseOriginProvider ShootingOrigin { get; set; }

		private float _currentCooldown;
		private bool _shouldActivate;

		public void Update()
		{
			_currentCooldown -= Time.deltaTime;
		}
		
		public void FixedUpdate()
		{
			if (_shouldActivate)
			{
				ActivateWithCd();
			}
		}
		
		private void ActivateWithCd()
		{
			if (_currentCooldown <= 0)
			{
				Activate();
				_currentCooldown = Cooldown;
			}
		}
		
		public void Activate(ISpawnEntityListener listener = null)
		{
			for (int i = 0; i < SpawnCount; i++)
			{
				var spawned = _objectFactory.CreateObject(SpawnId, ShootingOrigin.GetOrigin(OriginId).position, Quaternion.identity);
				if (spawned.TryGetComponent(out Rigidbody rb))
				{
					rb.AddForce(SpawnForce, ForceMode.Impulse);
				}
				
				listener?.OnEntitySpawned(spawned);
			}
		}

		public override void ItemInit(IOriginProxy inventory)
		{
			base.ItemInit(inventory);
			ShootingOrigin = inventory.Origin;
		}
	}
}