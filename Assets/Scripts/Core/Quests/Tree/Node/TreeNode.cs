using System.Collections;
using System.Collections.Generic;

namespace Core.Quests.Tree.Node
{
	public class TreeNode : ITreeNode
	{
		protected readonly Dictionary<string, TreeNode> Children =
			new Dictionary<string, TreeNode>();

		private readonly string _id;
		public string ID => _id;
		public TreeNode Parent { get; protected set; }

		public TreeNode(string id)
			=> _id = id;

		public TreeNode GetChild(string id)
			=> Children[id];

		public IReadOnlyDictionary<string, TreeNode> ReadOnlyChildren => Children;

		public void Add(TreeNode item)
		{
			item.Parent?.Children.Remove(item.ID);

			item.Parent = this;
			Children.Add(item.ID, item);
		}

		public IEnumerator<TreeNode> GetEnumerator()
			=> Children.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public int Count
			=> Children.Count;

		public override string ToString()
			=> ID;
	}

	public abstract class TreeNode<TValue> : TreeNode, ITreeNode<TValue>
	{
		private TValue _value;
		private readonly bool _hasValue;
		public TValue Value => _value;

		public TreeNode(string id, TValue value ) : base(id)
		{
			_value = value;
		}

		public virtual void SetValue(TValue value)
		{
			_value = value;
		}

	}

	public interface ITreeNode<out TValue> : ITreeNode
	{
		public TValue Value { get; }

		public string ToString();
	}

	public interface ITreeNode : IEnumerable<TreeNode>
	{
		public string ID { get; }
		public TreeNode Parent { get; }
		public IReadOnlyDictionary<string, TreeNode> ReadOnlyChildren { get; }
	}
}
