using System;
using Adv.Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shop.Models
{
	[Serializable]
	public class RemoveAdsModel : ShopItemModel, IRemoveAdsModel
	{
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/RemoveAds")]
		[SerializeField] private bool _isRemoveAds = true;
		
		public bool IsRemoveAds => _isRemoveAds;
		public bool ConstantlyRemoveAds => true;
	}

}