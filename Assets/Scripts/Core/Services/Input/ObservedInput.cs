using System;
using UniRx;
using UnityEngine;

namespace Core.Services.Input
{
	public interface IObservedInput : IDisposable
	{
		string KeyName { get; }
		void Observe(IInputService inputService);
	}
	public abstract class ObservedInput<T> : IObservedInput
	{
		public string KeyName { get; protected set; }
		public abstract IObservable<T> ReactiveProperty { get; }
		public abstract void Observe(IInputService inputService);
		public abstract void Dispose();
	}
	
	public class ObservedGetAxis : ObservedInput<float>
	{
		private readonly FloatReactiveProperty _reactiveAxis = new FloatReactiveProperty();

		public override IObservable<float> ReactiveProperty => _reactiveAxis;
		
		public ObservedGetAxis(string keyName)
		{
			KeyName = keyName;
		}

		public override void Observe(IInputService inputService)
		{
			_reactiveAxis.Value = inputService.GetAxis(KeyName);
		}
		
		public override void Dispose()
		{
			_reactiveAxis?.Dispose();
		}
	}
	
	public class ObservedGetAxis2D : ObservedInput<Vector2>
	{
		private readonly ReactiveProperty<Vector2> _reactiveAxis = new ReactiveProperty<Vector2>();

		public override IObservable<Vector2> ReactiveProperty => _reactiveAxis;
		public string KeyName2 { get; set; }

		public ObservedGetAxis2D(string keyName1, string keyName2)
		{
			KeyName = keyName1;
			KeyName2 = keyName2;
		}

		public override void Observe(IInputService inputService)
		{
			_reactiveAxis.Value = new Vector2(inputService.GetAxis(KeyName), inputService.GetAxis(KeyName2));
		}
		
		public override void Dispose()
		{
			_reactiveAxis?.Dispose();
		}
	}
	
#region ButtonObserved
	public class ObservedButton : ObservedInput<bool>
	{
		private readonly BoolReactiveProperty _boolReactive = new BoolReactiveProperty();
		public override IObservable<bool> ReactiveProperty => _boolReactive;

		public ObservedButton(string keyName)
		{
			KeyName = keyName;
		}

		public override void Observe(IInputService inputService)
		{
			_boolReactive.Value = inputService.GetButton(KeyName);
		}
		
		public override void Dispose()
		{
			_boolReactive?.Dispose();
		}
	}
	
	public class ObservedButtonDown : ObservedInput<Unit>
	{
		private readonly ReactiveCommand _boolReactive = new ReactiveCommand();
		
		public ObservedButtonDown(string keyName)
		{
			KeyName = keyName;
		}

		public override IObservable<Unit> ReactiveProperty => _boolReactive;
		
		public override void Observe(IInputService inputService)
		{
			if (inputService.GetButtonDown(KeyName))
			{
				_boolReactive.Execute();
			}
		}
		
		public override void Dispose()
		{
			_boolReactive?.Dispose();
		}
	}
	
	public class ObservedButtonUp : ObservedInput<Unit>
	{
		private readonly ReactiveCommand _boolReactive = new ReactiveCommand();
		
		public ObservedButtonUp(string keyName)
		{
			KeyName = keyName;
		}

		public override IObservable<Unit> ReactiveProperty => _boolReactive;
		
		public override void Observe(IInputService inputService)
		{
			if (inputService.GetButtonUp(KeyName))
			{
				_boolReactive.Execute();
			}
		}
		
		public override void Dispose()
		{
			_boolReactive?.Dispose();
		}
	}
#endregion
}