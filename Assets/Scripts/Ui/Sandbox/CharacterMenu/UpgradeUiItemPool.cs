using UnityEngine;
using uPools;

namespace Ui.Sandbox.CharacterMenu
{
	public class UpgradeUiItemPool : ObjectPoolBase<UpgradeUiItem>
	{
		private readonly UpgradeUiItem _prefab;
		private readonly Transform _parent;

		public UpgradeUiItemPool(UpgradeUiItem prefab, Transform parent)
		{
			_prefab = prefab;
			_parent = parent;
		}

		protected override void OnRent(UpgradeUiItem instance)
		{
			instance.UpgradeButton.gameObject.SetActive(true);
		}

		protected override UpgradeUiItem CreateInstance()
		{
			return Object.Instantiate(_prefab, _parent);
		}
	}
}