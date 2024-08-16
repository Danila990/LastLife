using UnityEngine;

namespace Shop.ShopUtils
{
	public static class PlatformUtils
	{
#if UNITY_IPHONE
		public static bool IsIPhone() => Application.platform is
			RuntimePlatform.IPhonePlayer or RuntimePlatform.OSXEditor or RuntimePlatform.WindowsEditor or RuntimePlatform.LinuxEditor;
#else
		public static bool IsIPhone() => Application.platform == RuntimePlatform.IPhonePlayer;
#endif

#if UNITY_ANDROID
		public static bool IsAndroid() => Application.platform is
			RuntimePlatform.Android or RuntimePlatform.OSXEditor or RuntimePlatform.WindowsEditor or RuntimePlatform.LinuxEditor;
#else
		public static bool IsAndroid() => Application.platform == RuntimePlatform.Android;
#endif

		public static bool IsEditor() => Application.platform is
			RuntimePlatform.OSXEditor or RuntimePlatform.WindowsEditor or RuntimePlatform.LinuxEditor;
	}
}