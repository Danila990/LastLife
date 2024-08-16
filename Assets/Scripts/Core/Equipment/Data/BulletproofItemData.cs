using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Equipment.Data
{
	[Serializable]
	public class BulletproofItemData : EquipmentItemData
	{
		[SerializeField]
		[InlineProperty]
		[HideLabel]
		[BoxGroup("Args")] private ArmorItemArgs _args;
		public override IEquipmentArgs Args => _args.GetCopy();
	}

}
