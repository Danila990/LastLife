using UnityEngine;
using uPools;

namespace Core.Quests.Tips.Impl
{
	public abstract class QuestTipObject : MonoBehaviour, IPoolCallbackReceiver
	{
		private ObjectPoolBase<QuestTipObject> _pool;
		
		public void OnCreated(ObjectPoolBase<QuestTipObject> pool)
		{
			_pool = pool;
		}
		
		protected virtual void OnCreatedInternal() { }
		
		
		public virtual void OnRent()
		{
			gameObject.SetActive(true);
		}
		
		public virtual void OnReturn()
		{
			if(!gameObject.Equals(null))
				gameObject.SetActive(false);
		}
		
		public void Release()
		{
			_pool.Return(this);
		}
	}

}
