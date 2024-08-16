// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2021 Dan's Game Tools
// http://DansGameTools.blogspot.com
// -------------------------------------------

using UnityEngine;

namespace ControlFreak2.Demos.Guns
{

	public class GunActivator : MonoBehaviour
	{
		public Gun[]
			gunList;

		public string
			buttonName = "Fire1";
		
		public KeyCode
			key = KeyCode.None;



		// -----------------
		private void Update()
		{
			var triggerState =
				key != KeyCode.None && CF2Input.GetKey(key) ||
				!string.IsNullOrEmpty(buttonName) && CF2Input.GetButton(buttonName);

			foreach (var g in gunList)
			{
				if (g != null)
					g.SetTriggerState(triggerState);
			}
		}
	}
}