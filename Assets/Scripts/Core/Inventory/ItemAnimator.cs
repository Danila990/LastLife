using Core.Entity.Characters.Adapters;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Inventory
{
	public abstract class ItemAnimator : MonoBehaviour
	{
		public ItemViewPresentationSettings ViewSettings;
		public Transform ModelHolder { get; protected set; }
		public abstract ItemModel RuntimeModel { get; }
		//public abstract ItemAnimationBehaviour AnimationBehaviour { get; }
		public abstract ref ItemAnimationBehaviour.RuntimeItemData RuntimeItemData { get; }

		public abstract void OnAdapterSet(IEntityAdapter adapter);
		public abstract void OnUpdate(float deltaTime);
		public abstract void SpawnModel();
		public abstract void EquipModel();
		public abstract void UnEquipModel();
		public abstract void TrySetAimState(bool state);
		public abstract void PlayAnim(string hash);
		public abstract void PlayAnim(int hash);
		public abstract UniTask<bool> PlayAnimAsync(int animHash, string triggerId);
		public abstract void SetFloat(int hash, float value);
	}
}