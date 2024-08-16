using System;
using Core.Boosts.Impl;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Db.ObjectData
{
	[Serializable]
	public class BoostObjectData : ObjectData
	{
		public string PickUpFactoryId;

		
		[field:SerializeField]
		[field:InlineProperty]
		[field:HideLabel]
		[field:BoxGroup(nameof(BoostArgs))]
		public BoostArgs BoostArgs { get; private set; }
	}

}