using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Core.AnimationRigging
{
	[Serializable]
	public struct RigData
	{
		public Rig Rig;
		public Transform Target;
		public float EnableTime;
		public float EnableDelay;
		public float DisableTime;
		public float DisableDelay;
	}
}