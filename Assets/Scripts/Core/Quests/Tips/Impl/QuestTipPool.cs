using System.Collections.Generic;
using UnityEngine;
using uPools;
using VContainer;

namespace Core.Quests.Tips.Impl
{
	public class QuestTipPool : ObjectPoolBase<QuestTipObject>
	{
		private readonly QuestTipObject _prefab;
		private readonly Transform _parent;
		private readonly Stack<QuestTipObject> _instances;

		public QuestTipPool(QuestTipObject prefab, Transform parent, IObjectResolver resolver)
		{
			_prefab = prefab;
			_parent = parent;
			_instances = new Stack<QuestTipObject>();
		}

		protected override QuestTipObject CreateInstance()
		{
			var instance = Object.Instantiate(_prefab, _parent);
			instance.OnCreated(this);
			return instance;
		}

		protected override void OnRent(QuestTipObject instance)
		{
			_instances.Push(instance);
		}

		protected override void OnReturn(QuestTipObject instance)
		{
			if (!instance)
				return;
			
			if(instance.transform.parent != _parent)
				instance.transform.SetParent(_parent);
		}

		public void ReturnAll()
		{
			while (_instances.Count > 0)
			{
				var instance = _instances.Pop();
				instance.Release();
			}
		}
	}
}
