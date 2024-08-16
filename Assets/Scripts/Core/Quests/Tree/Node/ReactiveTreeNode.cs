using System;
using UniRx;

namespace Core.Quests.Tree.Node
{
	public abstract class ReactiveTreeNode<TValue> : TreeNode<TValue>, IDisposable, IReactiveTreeNode
	{
		public abstract int MaxValue { get; }

		protected ReactiveTreeNode(string id, TValue value) : base(id, value) { }
		public abstract void Dispose();

		public abstract IDisposable Subscribe(Action<ReactiveIntNodeArgs> callback);


		public override string ToString()
			=> $"{base.ToString()}: {Value} : {MaxValue}";
		
	}

	public interface IReactiveTreeNode
	{
		public IDisposable Subscribe(Action<ReactiveIntNodeArgs> callback);
		public int MaxValue { get; }
	}
	
	public struct ReactiveIntNodeArgs
	{
		public readonly ReactiveIntTreeNode Node;
		public readonly int Value;

		public ReactiveIntNodeArgs(ReactiveIntTreeNode node, int value)
		{
			Node = node;
			Value = value;
		}
	}
}
