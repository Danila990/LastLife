using System;
using System.Collections.Generic;
using ControlFreak2;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Core.Services.Input
{
	public class ControlFreakInputService : IInputService, ITickable, IDisposable
	{
		private readonly SortedList<InputKey, IObservedInput> _observedInputs = new SortedList<InputKey, IObservedInput>();
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		
		public void Tick()
		{
			foreach (var observedInput in _observedInputs.Values)
			{
				observedInput.Observe(this);
			}
		}
		
		public IObservable<float> ObserveGetAxis(string axisName)
		{
			var inputKey = new InputKey(InputKeyType.Get, axisName);
			if (CheckCash<float>(ref inputKey, out var observable))
			{
				return observable;
			}
			
			var observed = new ObservedGetAxis(axisName).AddTo(_compositeDisposable);
			_observedInputs.Add(inputKey, observed);
			return observed.ReactiveProperty;
		}
		
		public IObservable<Vector2> ObserveGetAxis2D(string axisNameX, string axisNameY)
		{
			var key = new InputKey(InputKeyType.Get, axisNameX, axisNameY, false);
			if (CheckCash<Vector2>(ref key, out var observable))
			{
				return observable;
			}
			
			var observed = new ObservedGetAxis2D(axisNameX, axisNameY).AddTo(_compositeDisposable);
			_observedInputs.Add(key, observed);
			return observed.ReactiveProperty;
		}
		
		public IObservable<Vector2> ObserveGetAxis2DRaw(string axisNameX, string axisNameY)
		{
			var key = new InputKey(InputKeyType.Get, axisNameX, axisNameY, true);
			if (CheckCash<Vector2>(ref key, out var observable))
			{
				return observable;
			}
			
			var observed = new ObservedGetAxis2D(axisNameX, axisNameY).AddTo(_compositeDisposable);
			_observedInputs.Add(key, observed);
			return observed.ReactiveProperty;
		}

#region Buttons
		public IObservable<Unit> ObserveGetButtonDown(string buttonName)
		{
			var key = new InputKey(InputKeyType.GetDown, buttonName);
			if (CheckCash<Unit>(ref key, out var observable))
			{
				return observable;
			}
			
			var observed = new ObservedButtonDown(buttonName).AddTo(_compositeDisposable);
			_observedInputs.Add(key, observed);
			return observed.ReactiveProperty;
		}
		
		public IObservable<Unit> ObserveGetButtonUp(string buttonName)
		{
			var key = new InputKey(InputKeyType.GetUp, buttonName);
			if (CheckCash<Unit>(ref key, out var observable))
			{
				return observable;
			}
			var observed = new ObservedButtonUp(buttonName).AddTo(_compositeDisposable);
			_observedInputs.Add(key, observed);
			return observed.ReactiveProperty;
		}

		public IObservable<bool> ObserveGetButton(string buttonName)
		{
			var key = new InputKey(InputKeyType.Get, buttonName);
			if (CheckCash<bool>(ref key, out var observable))
			{
				return observable;
			}
			
			var observed = new ObservedButton(buttonName).AddTo(_compositeDisposable);
			_observedInputs.Add(key, observed);
			return observed.ReactiveProperty;
		}
		  #endregion

		
		public float GetAxis(string axisName) => CF2Input.GetAxis(axisName);
		public float GetAxisRaw(string axisName) => CF2Input.GetAxisRaw(axisName);
		public bool GetButtonDown(string buttonName) => CF2Input.GetButtonDown(buttonName);
		public bool GetButton(string buttonName) => CF2Input.GetButton(buttonName);
		public bool GetButtonUp(string buttonName) => CF2Input.GetButtonUp(buttonName);
		
		private bool CheckCash<T>(ref InputKey inputKey, out IObservable<T> observable)
		{
			if (_observedInputs.TryGetValue(inputKey, out var observedInput))
			{
				observable = ((ObservedInput<T>)observedInput).ReactiveProperty;
				return true;
			}
			
			observable = null;
			return false;
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
		
		private readonly struct InputKey : IComparable<InputKey>, IEqualityComparer<InputKey>
		{
			public readonly InputKeyType Type;
			public readonly int SecondHash;

			public InputKey(InputKeyType type, string name)
			{
				Type = type;
				SecondHash = name.GetHashCode();
			}
			
			public InputKey(InputKeyType type, string axisX, string axisY, bool raw)
			{
				Type = type;
				SecondHash = HashCode.Combine(axisX.GetHashCode(), axisY.GetHashCode(), raw.GetHashCode());
			}

			public override int GetHashCode()
			{
				return HashCode.Combine((byte)Type, SecondHash.GetHashCode());
			}
			
			public int GetHashCode(InputKey obj)
			{
				return HashCode.Combine((byte)obj.Type, obj.SecondHash);
			}
			
			public bool Equals(InputKey x, InputKey y)
			{
				return x.Type == y.Type && x.SecondHash == y.SecondHash;
			}
			
			public int CompareTo(InputKey other)
			{
				var typeComparison = Type.CompareTo(other.Type);
				if (typeComparison != 0)
					return typeComparison;
				return SecondHash.CompareTo(other.SecondHash);
			}
		}
	}

	public enum InputKeyType : byte
	{
		GetDown,
		Get,
		GetUp
	}
	
	public interface IInputService
	{
		float GetAxis(string axisName);
		float GetAxisRaw(string axisName);
		bool GetButtonDown(string buttonName);
		bool GetButton(string buttonName);
		bool GetButtonUp(string buttonName);
		IObservable<float> ObserveGetAxis(string axisName);
		IObservable<Vector2> ObserveGetAxis2D(string axisNameX, string axisNameY);
		IObservable<Vector2> ObserveGetAxis2DRaw(string axisNameX, string axisNameY);
		IObservable<Unit> ObserveGetButtonDown(string buttonName);
		IObservable<Unit> ObserveGetButtonUp(string buttonName);
		IObservable<bool> ObserveGetButton(string buttonName);
	}
}