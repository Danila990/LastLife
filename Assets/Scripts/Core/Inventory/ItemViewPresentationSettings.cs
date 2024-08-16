using Core.Inventory.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Inventory
{
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(ItemViewPresentationSettings), fileName = nameof(ItemViewPresentationSettings))]
	public class ItemViewPresentationSettings : ScriptableObject
	{
		[TitleGroup("MainSettings")] public bool CanFPV;
		[TitleGroup("MainSettings")] public bool CanTPV;
		[TitleGroup("MainSettings")] public bool UseFpvHands;
		[TitleGroup("MainSettings")] public bool PlaceInHand;
		[ShowIf("PlaceInHand"), TitleGroup("MainSettings")] public bool PlaceInLeft;

		[ShowIf("CanTPV"), BoxGroup("MainSettings/ThirdPerson")] public ItemIKGripType GripType;
		[ShowIf("CanTPV"), BoxGroup("MainSettings/ThirdPerson")] public ItemModel ItemModel;
		
		[ShowIf("CanTPV"), BoxGroup("MainSettings/ThirdPerson")] public HumanBodyBones ItemBone;
		
		[ShowIf("CanTPV"), BoxGroup("MainSettings/ThirdPerson")] public Vector3 ModelOffset;
		[ShowIf("CanTPV"), BoxGroup("MainSettings/ThirdPerson")] public Vector3 ModelRotation;
		
		[ShowIf("CanFPV"), BoxGroup("MainSettings/FirstPerson")] public bool SeparateFpvModel;
		[ShowIf("@SeparateFpvModel && CanFPV"), BoxGroup("MainSettings/FirstPerson")] public ItemModel FpvModel;
		
		[ShowIf("CanFPV"), BoxGroup("MainSettings/FirstPerson")] public Vector3 FpvModelOffset;
		[ShowIf("CanFPV"), BoxGroup("MainSettings/FirstPerson")] public Vector3 FpvModelRotation;
		[ShowIf("CanFPV"), BoxGroup("MainSettings/FirstPerson")] public Vector3 FpvModelScale;
		
		[ShowIf("UseFpvHands"), BoxGroup("MainSettings/FirstPerson")] public Vector3 FpvHandPos;
		[ShowIf("UseFpvHands"), BoxGroup("MainSettings/FirstPerson")] public Vector3 FpvHandScale;
		
		[ShowIf("CanFPV"), BoxGroup("MainSettings/FirstPerson")] public AnimationBehaviour[] Behaviours;
		[ShowIf("CanFPV"), BoxGroup("MainSettings/FirstPerson")] public string OverrideIdle;
		[ShowIf("CanFPV"), BoxGroup("MainSettings/FirstPerson")] public bool NeedShowHands;
	}
}