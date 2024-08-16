using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Core.Boosts;
using Core.Boosts.Impl;
using Core.Entity.Characters.Adapters;
using UniRx;
using UnityEngine;

namespace Core.Entity.Characters
{
	public class StatsProvider : IDisposable
	{
		private readonly BaseCharacterAdapter _adapter;
		private readonly Stats _stats;
		private readonly CompositeDisposable _disposable;
		private readonly Dictionary<string, IDisposable> _boostedStats;

		public Stats Stats => _stats;

		public StatsProvider(IBoostProvider boostProvider, BaseCharacterAdapter adapter)
		{
			_adapter = adapter;
			_stats = new();
			_disposable = new();
			_boostedStats = new();
			
			boostProvider.OnBoostApplied.Subscribe(OnBoostApplied).AddTo(_disposable);
			boostProvider.OnBoostEnded.SubscribeWithState(false, OnBoost).AddTo(_disposable);
			adapter.ContextChanged.Subscribe(_ => OnContextChanged()).AddTo(_disposable);
		}

		private void OnBoostApplied(AppliedBoostArgs appliedBoostArgs)
			=> OnBoost(appliedBoostArgs.BoostArgs, true);

		private void OnBoost(BoostArgs args, bool isApplied)
		{
			switch (args.Type)
			{
				case BoostTypes.SPEED_UP:
					ChangeStats(BoostTypes.SPEED_UP, StatType.MovementSpeed, args.Value, isApplied);
					break;
				
				case BoostTypes.JUMP_UP:
					ChangeStats(BoostTypes.JUMP_UP, StatType.JumpForce, args.Value, isApplied);
					break;
				
				case BoostTypes.DAMAGE:
					ChangeStats(BoostTypes.DAMAGE, StatType.AllDamage, args.Value, isApplied);
					break;
				
				case BoostTypes.HP:
					break;
				
				default:
					throw new NotImplementedException($"Not Implemented {args.Type}");
			}
		}

		private void ChangeStats(string boostType, StatType type, float value, bool increase)
		{
			if (increase && !_boostedStats.ContainsKey(boostType))
			{
				_boostedStats.Add(boostType, _stats.IncreaseStats(type, value));
			}
			else if (_boostedStats.TryGetValue(boostType, out var disposable))
			{
				disposable.Dispose();
				_boostedStats.Remove(boostType);
			}
		}

		private void OnContextChanged()
		{
			foreach (var stat in _boostedStats.Values)
				stat.Dispose();
			
			_boostedStats.Clear();	
		}
		
		public void Dispose()
		{
			foreach (var stat in _boostedStats.Values)
				stat.Dispose();
			
			_boostedStats.Clear();
			_disposable?.Dispose();
		}
	}
	
	public class Stats
	{
		private readonly Dictionary<StatType, float> _values = new();

		public IDisposable IncreaseStats(StatType type, float value)
		{
			if (!_values.TryAdd(type, value))
				_values[type] += value;
			return new StatUser(this, type, value);
		}
		

		public float GetValue(StatType type)
			=> _values.GetValueOrDefault(type, 0f);

		public bool GetValue(StatType type, out float value)
			=> _values.TryGetValue(type, out value);

		private class StatUser : IDisposable
		{
			private readonly Stats _stats;
			private readonly StatType _type;
			private readonly float _value;
			private bool _isDisposed; 
			
			public StatUser(Stats stats, StatType type, float value)
			{
				_stats = stats;
				_type = type;
				_value = value;
				_isDisposed = false;
			}

			public void Dispose()
			{
				if (_isDisposed)
					return;
				
				_isDisposed = true;
				if (_stats._values.ContainsKey(_type))
				{
					_stats._values[_type] -= _value;
				}
			}
		}
	}


	public enum StatType
	{
		MovementSpeed = 0,
		JumpForce = 1,
		AllDamage = 2,
		MeleeDamage = 3,
		FallSpeedLimit = 4,
		MeleeHitForce = 5,
	}
}
