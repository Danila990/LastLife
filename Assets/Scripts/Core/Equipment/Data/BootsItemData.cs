using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Equipment.Data
{
	[Serializable]
	public class BootsItemData : EquipmentItemData
	{
		[SerializeField]
		[InlineProperty]
		[HideLabel]
		[BoxGroup("Args")]
		private BootsItemArgs _args;
		public override IEquipmentArgs Args => _args.GetCopy();
	}

}
