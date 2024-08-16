using System;
using Adv.Services;
using Core.ResourcesSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shop.Models
{
	[Serializable]
	public class TicketModel : ShopItemModel, IRemoveAdsModel
	{
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		public int TicketCount;

		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/RemoveAds")]
		[SerializeField] private bool _isRemoveAds;
		
		[TitleGroup("Info", alignment: TitleAlignments.Centered)]
		[HorizontalGroup("Info/Split")]
		[VerticalGroup("Info/Split/Left")]
		[BoxGroup("Info/Split/Left/Base")]
		public bool IsBestSeller;
		
		[Space(20)]
		public ResourceType ResourceType = ResourceType.Ticket;

		[field:Space(20)]
		public bool IsRemoveAds => _isRemoveAds;
		public bool ConstantlyRemoveAds => true;
	}
}