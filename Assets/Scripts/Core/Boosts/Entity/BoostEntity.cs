using System.Linq;
using System.Threading;
using Core.Boosts.Inventory;
using Core.Inventory;
using Core.Inventory.Items;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Core.Boosts.Entity
{
	public class BoostEntity : ItemContext
	{
		private StoredBoost _selectedBoost;
		public AnimationClip UseClip;
		public string UseFpvName;
		private bool _inUse;
		private BoostItemAnimator _boostItemAnimator;
		private int _selectedBoostIndex;
		
		private BoolReactiveProperty _canSwitch;
		public IReactiveProperty<bool> CanSwitch => _canSwitch;

		public override void ItemInit(IOriginProxy inventory)
		{
			base.ItemInit(inventory);
			_boostItemAnimator = ItemAnimator as BoostItemAnimator;
			_canSwitch = new BoolReactiveProperty(false).AddTo(this);
		}

		[Button]
		public void SwitchBoost()
		{
			_selectedBoostIndex++;
			if (_selectedBoostIndex >= Adapter.BoostProvider.BoostsInventory.Boosts.Count)
			{
				_selectedBoostIndex = 0;
			}
			OnSelectedBoostChanged();
		}

		public override void OnSelect()
		{
			base.OnSelect();
			OnSelectedBoostChanged();
		}

		private void OnSelectedBoostChanged()
		{
			if (Adapter.BoostProvider.BoostsInventory.Boosts.Count <= 0)
				return;
			UpdateView();
			ApplyView();
		}

		public void UpdateView()
		{
			_selectedBoost = Adapter.BoostProvider.BoostsInventory.Boosts.ElementAt(_selectedBoostIndex).Value;
			CurrentQuantity.Value = Adapter.BoostProvider.BoostsInventory.Boosts.Sum(x => x.Value.Quantity);
			_canSwitch.Value = CurrentQuantity.Value > 1;
		}

		private void ApplyView()
		{
			_boostItemAnimator.OnBoostChanged(_selectedBoost.Args.Type);
		}

		[Button]
		public void ApplyBoost()
		{
			Adapter.BoostProvider.ApplyBoost(_selectedBoost.Args.Type);
			if (Adapter.BoostProvider.BoostsInventory.Boosts.TryGetValue(_selectedBoost.Args.Type, out _selectedBoost))
			{
				CurrentQuantity.Value = Adapter.BoostProvider.BoostsInventory.Boosts.Sum(x => x.Value.Quantity);
			}
			else
			{
				SwitchBoost();
			}
		}
		
		[Button]
		public void ApplyBoostAnimated()
		{
			ApplyBoostAsync(destroyCancellationToken).Forget();
		}

		private async UniTaskVoid ApplyBoostAsync(CancellationToken token)
		{
			if (_inUse)
				return;
			
			_inUse = true;
			var used = await Adapter.CharacterAnimatorAdapter.PlayAction(UseClip, UseFpvName, "BoostTrigger", 0.5f);
			_inUse = false;

			if (!used)
				return;
					
			ApplyBoost();
		}
	}
}
