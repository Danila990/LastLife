using System;
using Core.Boosts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Inventory
{
	public class BoostItemAnimator : SimpleItemAnimator
	{
		public SyringeBoostView[] BoostsViews;

		private SyringeItemModel TpvSyringe { get; set; }
		private SyringeItemModel FpvSyringe { get; set; }

		private void Awake()
		{
			for (int i =  0; i  < BoostsViews.Length; i++)
			{
				var boostView = BoostsViews[i];
				boostView.BoostMaterial = new Material(boostView.BoostMaterial);
				BoostsViews[i] = boostView;
			}
		}

		public override void SpawnModel()
		{
			base.SpawnModel();
			if (ThpModel)
			{
				TpvSyringe = (SyringeItemModel)ThpModel;
			}
			if (FpvModel)
			{
				FpvSyringe = (SyringeItemModel)FpvModel;
			}
		}
		
		public void OnBoostChanged(string newBoost)
		{
			ref var view = ref GetView(newBoost);
			if (TpvSyringe)
			{
				TpvSyringe.SyringeLiquidRenderer.material = view.BoostMaterial;
				TpvSyringe.SyringeLiquid.SetMaterial(view.BoostMaterial);
			}
			if (FpvSyringe)
			{
				FpvSyringe.SyringeLiquidRenderer.material = view.BoostMaterial;
				FpvSyringe.SyringeLiquid.SetMaterial(view.BoostMaterial);
			}
		}

		private ref SyringeBoostView GetView(string newBoost)
		{
			for (var i = 0; i < BoostsViews.Length; i++)
			{
				if (BoostsViews[i].BoostId == newBoost)
				{
					return ref BoostsViews[i];
				}
			}
			
			throw new ArgumentOutOfRangeException(newBoost, $"doesnt contains preview for {newBoost}");
		}
		
		[Serializable]
		public struct SyringeBoostView
		{
			[ValueDropdown("@BoostTypes.GetTypes()")]
			[GUIColor("GetColor")]
			public string BoostId;
			public Material BoostMaterial;
			
#if UNITY_EDITOR
			private Color GetColor()
			{
				return BoostTypes.GetColorByType(BoostId);
			}
#endif
		}
	}
}