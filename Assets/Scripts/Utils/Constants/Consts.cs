using System;
using System.Collections.Generic;
using System.Linq;
using Core.Actions;

namespace Utils.Constants
{
	public static class AnimTriggers
	{
		public const string LANDING_EXIT = "landing_exit";
	}

	public static class VFXConsts
	{
#if UNITY_EDITOR
		public static IEnumerable<string> GetKeys()
		{
			var res = UnityEditor.AssetDatabase
				.LoadAssetAtPath<Core.Factory.VFXFactory.Impl.VFXData>("Assets/Settings/Data/VFX/VFXData.asset")
				.ParticleData.Select(x => x.Name).ToList();
			res.Add("");
			return res;
			/*var assets = AssetDatabase.FindAssets("t:VFXData", null);
			var res = new List<string>();
			foreach (var asset in assets)
			{
				var vfxPath = AssetDatabase.GUIDToAssetPath(asset);
				var vfxData = AssetDatabase.asset<VFXData>(vfxPath);
				var vfxKeys = vfxData.ParticleData.Select(x => x.Name);
				res.AddRange(vfxKeys);
			}
			return res;*/
		}
#endif
	}
	
	public static class InputConsts
	{
		public const string HORIZONTAL = "Horizontal";
		public const string VERTICAL = "Vertical";
		public const string MOUSE_X = "Mouse X";
		public const string MOUSE_Y = "Mouse Y";
		public const string JUMP = "Jump";
		public const string SPRINT = "Sprint";
		public const string AIM = "Aim";
		public const string MAIN_ACTION = "MainAction";
		public const string ACTION_ONE = "ActionOne";
		public const string ACTION_TWO = "ActionTwo";
		public const string ACTION_THIRD = "ActionThird";
		public const string ACTION_FOUR = "ActionFour";
		public const string SWITCH_WEAPON = "SwitchWeapon";
		public const string INTERACT = "Interact";

		public static string ToAxisName(this ActionKey actionKey) => actionKey switch
		{
			ActionKey.MainAction => MAIN_ACTION,
			ActionKey.ActionOne => ACTION_ONE,
			ActionKey.ActionTwo => ACTION_TWO,
			ActionKey.ActionThird => ACTION_THIRD,
			ActionKey.ActionFour => ACTION_FOUR,
			ActionKey.None => "",
			ActionKey.AimButton => AIM,
			_ => throw new ArgumentOutOfRangeException(nameof(actionKey), actionKey, null)
		};
	}
	
	public static class Consts
	{
		public const string CURRENT_SANDBOX_MAP = "CURRENT_SANDBOX_SELECTED_MAP";
		public const string DEFAULT_SANDBOX_MAP = "Default_Sandbox_Map";
		public const string AUTOSAVE_NAME = "autosave";
		public const string SPIN_CHARACTER_KEY = "SPIN_CHARACTER_KEY";

		public const string SKIP_INTERSTITIAL = "SkipInterstitial";
	}
	
	public static class WeaponConsts
	{
		public const string CAPACITY = "Capacity";
		public const string DAMAGE = "Damage";
		public const string ACCURACY = "Accuracy";
		public const string DISTANCE = "Distance";
		public const string FALLOFF = "Falloff";
		public const string SPIN = "Spin";
		public const string FIRE_RATE = "FireRate";
	}
	
	public static class SettingsConsts
	{
		public const string MAIN_SETTINGS_NAME = "Main_Settings";
		public const string QUALITY_SETTINGS = "Quality_Settings";
		
		public const string BOSS_SEQUENCE = "BossSequence";
		public const string BOOST_PRICE = "BoostPrice";
		public const string ALT_INTERACTION = "AltInter";
		public const string REMOVE_AFTER_DEATH = "RemoveAfterDeath";
		public const string REMOVE_AFTER_DEATH_TIMER = "REMOVE_AFTER_DEATH_TIMER";
		public const string MAX_BOT_SPAWNED_COUNT = "MAX_BOT_SPAWNED_COUNT";
		public const string REMOVE_MAP_EXTERIOR = "REMOVE_MAP_EXTERIOR";
		public const string REMOVE_INITIAL_CHARACTERS = "REMOVE_INITIAL_CHARACTERS";
		public const string HD_MOD = "hd_mod";
		public const string IN_PLAYER_WINDOW_IAP = "InPlayerWindowIAP";
		public const string ADDITIONAL_ADV_FEATURES = "AdditionalAdvFeatures";
	}
}