using System;
using Db.Roulette;
using Sirenix.OdinInspector;
using Ui.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Sandbox.CharacterMenu.Roulette
{
	[Serializable]
	public class RouletteData
	{
		[TitleGroup("Roulette")]
		[AssetList(Path = "Settings/Data/RouletteObjects", AssetNamePrefix = "Roulette"), Searchable] 
		public RouletteObjectSo[] ObjectsData;
		public Button OpenShop;
		public RectTransform Content;
		public RectTransform RouletteHolder;
		public RectTransform ScrollView;
		public HighlightableImage ItemPrefab;
		public HorizontalLayoutGroup LayoutGroup;
		public Image PointerImg;

		[TitleGroup("Control")]
		public Button SpinButton;
		public ButtonWidget SpinButtonTicket;
		public Button TakeButton;
		public ButtonWidget TakeButtonTicket;
		public Button HideOrShowButton;
		public RectTransform RouletteUpDownImg;
		
		[ReadOnly] public int ElementsCount;
		[TitleGroup("Settings")]
		public float Acceleration;
	}
}