using System;
using UniRx;
using UnityEngine;

namespace Core.Quests.Tree.Node
{
	public class ReactiveIntTreeNode : ReactiveTreeNode<int>
	{
		private readonly int _maxValue;
		private readonly ReactiveCommand<ReactiveIntNodeArgs> _command;


		public override int MaxValue => _maxValue;

		public ReactiveIntTreeNode(string id, int maxValue, int initialValue) : base(id, initialValue)
		{
			_maxValue = maxValue;
			_command = new ReactiveCommand<ReactiveIntNodeArgs>();
		}

		public override void SetValue(int value)
		{
			base.SetValue(Mathf.Clamp(Value + value, 0, _maxValue));
			ExecuteCommand();
		}

		public override IDisposable Subscribe(Action<ReactiveIntNodeArgs> callback)
		{
			if (Value >= MaxValue)
			{
				callback(new ReactiveIntNodeArgs(this, Value));
			}

			return _command.Subscribe(callback);
		}

		private void ExecuteCommand()
		{
			if (!_command.IsDisposed)
				_command.Execute(new ReactiveIntNodeArgs(this, Value));
			if (Value >= _maxValue && !_command.IsDisposed)
				Dispose();
		}

		public override void Dispose()
		{
			_command?.Dispose();
		}
	}

}
