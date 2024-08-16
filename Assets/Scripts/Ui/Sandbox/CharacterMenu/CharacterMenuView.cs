using System;
using Sirenix.OdinInspector;
using TMPro;
using Ui.Sandbox.CharacterMenu.Roulette;
using Ui.Sandbox.SelectMenu;
using Ui.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Sandbox.CharacterMenu
{
	public class CharacterMenuView : SelectMenuMainView
	{
		[TitleGroup("CharacterSelecting")]
		public OutlineButtonWidgetWithNumber SelectCharacterPrefab;
		public Transform Characters4SelectHolder;
		public MainCharacterPreview MainCharacterPreview;
		
		[BoxGroup("CharacterSelecting/Lvl")]
		public Image SelectedCharacterLvlImage;
		
		[BoxGroup("CharacterSelecting/Lvl")] 
		public TextMeshProUGUI SelectedCharacterLvlNumber;
		[BoxGroup("CharacterSelecting/Lvl")] 
		public TextMeshProUGUI SelectedCharacterLvlTXT;
		
		[HorizontalGroup("CharacterSelecting/CharacterName")]
		public TextMeshProUGUI CharNameTXT;
		
		[HorizontalGroup("CharacterSelecting/CharacterName")] 
		public string CharNameFormat;
		
		[HorizontalGroup("CharacterSelecting/Buttons")] 
		public Button PlayBTN;

		[HorizontalGroup("CharacterSelecting/Buttons")] 
		public Button BuyBTN;
		
		[HorizontalGroup("CharacterSelecting/Buttons")] 
		public TextMeshProUGUI BuyBTNTxt;
		
		[BoxGroup("CharacterSelecting/Sprites")]
		public Sprite LockSprite;

		
		[BoxGroup("Switch")] public CharacterScreenData CharacterScreen;
		[BoxGroup("Switch")] public CharacterScreenData SkillsScreen;
		
		[FoldoutGroup("Skills")]
		[InlineProperty]
		[HideLabel]
		public SkillsScreenData SkillsScreenData;
		
		[FoldoutGroup("RouletteData")]
		[InlineProperty]
		[HideLabel]
		public RouletteData RouletteData;
		
		[FoldoutGroup("EquipmentData")]
		[InlineProperty]
		[HideLabel]
		public EquipmentMenuData EquipmentMenuData;
	}

	[Serializable]
	public class EquipmentMenuData
	{
		public Sprite PlusSprite;
		public EquipmentButtonWidget[] Widgets;
		public ButtonWidget DeselectButton;
		public Button SkillsButtons;
		public Button HideButton;
		public Button CrossButton;
		public GameObject Equipment;
	}
	
	[Serializable]
	public struct CharacterScreenData
	{
		public GameObject Screen;
		public ButtonWidget Widget;
	}
}