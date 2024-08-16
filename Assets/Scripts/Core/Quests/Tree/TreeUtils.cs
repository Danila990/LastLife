using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Quests.Tree.Node;
using Db.Quests;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Quests.Tree
{
	public static class TreeUtils
	{
		public static string BuildString(this ITreeNode tree)
		{
			var sb = new StringBuilder();

			BuildString(sb, tree, 0);

			return sb.ToString();
		}
		private static void BuildString(StringBuilder sb, ITreeNode node, int depth)
		{
			sb.AppendLine(node.ToString().PadLeft(node.ToString().Length + depth));

			foreach (var child in node)
			{
				BuildString(sb, child, depth + 1);
			}
		}
		
		public static IEnumerable<TreeNode> BreadthFirstSearch(this TreeNode root)
		{
			var visited = new HashSet<TreeNode>();
			var queue = new Queue<TreeNode>();

			queue.Enqueue(root);
			visited.Add(root);

			while (queue.Count > 0)
			{
				var currentNode = queue.Dequeue();
				yield return currentNode;

				foreach (var childNode in currentNode.ReadOnlyChildren.Values.Where(child => !visited.Contains(child)))
				{
					queue.Enqueue(childNode);
					visited.Add(childNode);
				}
			}
		}
		
		public static IEnumerable<TreeNode> DepthFirstSearch(this TreeNode root)
		{
			var visited = new HashSet<TreeNode>();
			var stack = new Stack<TreeNode>();

			stack.Push(root);
			visited.Add(root);

			while (stack.Count > 0)
			{
				var currentNode = stack.Pop();
				yield return currentNode;

				foreach (var childNode in currentNode.ReadOnlyChildren.Values.Where(child => !visited.Contains(child)))
				{
					stack.Push(childNode);
					visited.Add(childNode);
				}
			}
		}
		public static void RecursiveTraverse(this TreeNode root, Action<TreeNode> action)
		{
			action(root);

			foreach (var child in root.ReadOnlyChildren.Values)
			{
				child.RecursiveTraverse(action);
			}
		}

		public static void RecursiveTraverse(this TreeNode root, string targetNodeId, Action<TreeNode> callback)
		{
			root.RecursiveTraverse(node =>
			{
				if (node.ID == targetNodeId)
				{
					callback(node);
				}
			});
		}

		public static (TreeNode LastNode, ICollection<string> MissingIds) FindLastNodeInSimilarBranchWithMissing(this TreeNode root, params string[] targetNodeIds)
		{
			var lastNode = root;
			var missingIds = new List<string>(targetNodeIds);
			
			foreach (var id in targetNodeIds)
			{
				if (root.ReadOnlyChildren.ContainsKey(id))
				{
					missingIds.Remove(id);
					root = root.GetChild(id);
					lastNode = root;
				}
				else
				{
					break;
				}
			}

			return (lastNode, missingIds);
		}
		public static TreeNode FindLastNodeInSimilarBranch(this TreeNode root, params string[] targetNodeIds)
		{
			TreeNode lastNode = null;
			var currentNode = root;

			foreach (var id in targetNodeIds)
			{
				if (currentNode.ReadOnlyChildren.ContainsKey(id))
				{
					currentNode = currentNode.GetChild(id);
					lastNode = currentNode;
				}
				else
				{
					break;
				}
			}

			return lastNode;
		}
		public static TreeNode AddBranchOrNodes(this TreeNode root, string[] targetNodeIds)
		{
			var lastNodeInSimilarBranch = root.FindLastNodeInSimilarBranch(targetNodeIds);
			TreeNode currentNode = null;
				
			if (lastNodeInSimilarBranch == null)
			{
				currentNode = root;
				foreach (var id in targetNodeIds)
				{
					var newNode = new TreeNode(id);
					currentNode.Add(newNode);
					currentNode = newNode;
				}
			}
			else if (lastNodeInSimilarBranch.ID != targetNodeIds[^1])
			{
				currentNode = lastNodeInSimilarBranch;
				for (var i = Array.IndexOf(targetNodeIds, lastNodeInSimilarBranch.ID) + 1; i < targetNodeIds.Length; i++)
				{
					var newNode = new TreeNode(targetNodeIds[i]);
					currentNode.Add(newNode);
					currentNode = newNode;
				}
			}

			return currentNode;
		}

		private const string INLINE_ID_FORMAT = "{0}/"; 
		private const string FINAL_ID_FORMAT = "{0}"; 
		
		public static string CreateInlineId(params string[] ids)
		{
			var stringBuilder = new StringBuilder();
			for (var i = 0; i < ids.Length; i++)
			{
				var id = ids[i];
				if (string.IsNullOrEmpty(id))
					continue;
				var format = i == ids.Length - 1 ? FINAL_ID_FORMAT : INLINE_ID_FORMAT;
				stringBuilder.Append(string.Format(format, id));
			}

			return stringBuilder.ToString();
		}
		
		
		public static string CreateInlineId(string rootId, IEnumerable<string> midIds, string finalId)
		{
			var list = ListPool<string>.Get();
			list.Add(rootId);
			list.AddRange(midIds);
			list.Add(finalId);
			string inlineId = CreateInlineId(list.ToArray());
			ListPool<string>.Release(list);
			return inlineId;
		}
		public static string CreateInlineId(string rootId, IEnumerable<QuestNode> midIds, string finalId)
		{
			var list = ListPool<string>.Get();
			list.Add(rootId);
			list.AddRange(midIds.Select(x => x.Id));
			list.Add(finalId);
			string inlineId = CreateInlineId(list.ToArray());
			ListPool<string>.Release(list);
			return inlineId;
		}
		
		public static string CreateInlineId(IEnumerable<string> midIds, string finalId)
		{
			var list = ListPool<string>.Get();
			list.AddRange(midIds);
			list.Add(finalId);
			string inlineId = CreateInlineId(list.ToArray());
			ListPool<string>.Release(list);
			return inlineId;
		}
		
		public static string GetInlineId(this ITreeNode finalNode)
		{
			var stringBuilder = new StringBuilder();
			var currentNode = finalNode;
			var queue = new Stack<ITreeNode>();
			while (currentNode != null)
			{
				if (currentNode == currentNode.Parent)
					break;
				
				queue.Push(currentNode);
				currentNode = currentNode.Parent;
			}
			while (queue.Count > 0)
			{
				var format = queue.Count == 1 ? FINAL_ID_FORMAT : INLINE_ID_FORMAT;
				stringBuilder.Append(string.Format(format, queue.Pop().ID));
			}
			
			return stringBuilder.ToString();
		}

	}
}
