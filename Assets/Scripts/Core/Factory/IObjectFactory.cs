using System;
using Core.Entity;
using Core.Entity.Characters.Adapters;
using Core.Equipment;
using Core.Equipment.Impl;
using Core.Inventory.Items;
using Core.Loot;
using Core.Projectile;
using Core.Projectile.Projectile;
using Db.ObjectData;
using UnityEngine;

namespace Core.Factory
{
	public interface IObjectFactory
	{
		EntityContext CreateObject(string key, Vector3 pos, Quaternion rot, bool addToRepository = true);
		EntityContext CreateObject(string key, Vector3 pos, bool addToRepository = true);
		EntityContext CreateObject(string key);
		EntityContext CreateFromPrefab(EntityContext prefab, string key) => null;
		Transform Holder { get; }
	}

	public interface IAiCharacterFactory : IAiAdapterFactory
	{
		AiCharacterAdapter CreateObject(string key, Vector3 pos, Quaternion rot);
		AiCharacterAdapter CreateObject(string key, Vector3 pos);
		AiCharacterAdapter CreateObject(string key);
	}
	
	public interface IAiHeadFactory : IAiAdapterFactory
	{
		
	}

	public interface IAiAdapterFactory
	{
		IEntityAdapter Create(string key);
		IEntityAdapter Create(string key, Vector3 pos, Quaternion rotation = default);
		public Type AdapterType { get; }
		public AiType AiType { get; }
	}

	public interface IPlayerCharacterFactory
	{
		PlayerCharacterAdapter CreatePlayerAdapterOnly(Vector3 pos = default, Quaternion spawnPointRotation = default);
	}
	
	public interface IMechAdapterFactory
	{
		MechCharacterAdapter CreateMechAdapter(Vector3 pos = default, Quaternion spawnPointRotation = default);
	}

	public interface IProjectileFactory
	{
		ProjectileEntity CreateObject(string key, Vector3 pos, Quaternion rot);
		ProjectileEntity CreateObject(string key, Vector3 pos);
		ProjectileEntity CreateObject(string key);
		ProjectileEntity CreateObject(ref ProjectileCreationData creationData);
		float GetProjectileMass(string key);
	}

	public interface IThrowableFactory
	{
		ThrowableEntity CreateObject(string key, Vector3 pos, Quaternion rot);
		ThrowableEntity CreateObject(string key, Vector3 pos);
		ThrowableEntity CreateObject(string key);
		ThrowableEntity CreateObject(ref ThrowableCreationData creationData);
	}
	
	public interface IEquipmentFactory
	{
		EquipmentEntityContext CreateObject(string key, bool isFat = false);
		public EquipmentEntityContext CreateFromPrefab(EquipmentEntityContext prefab, string key) => null;

	}
	
	public interface ILootFactory
	{
		LootEntity CreateObject(string key);
		LootEntity CreateObject(string key, Vector3 position);
		LootEntity CreateObjectAroundPoint(string key, Vector3 position, float radius = 2f);
		
	}
}