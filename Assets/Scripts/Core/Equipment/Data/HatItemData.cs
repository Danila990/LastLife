using System;
using Core.Equipment.Data;
using UnityEngine;

namespace Core.Equipment
{
	[Serializable]
	public class HatItemData : EquipmentItemData
	{
		[SerializeField] private HatItemArgs _args;
		public override IEquipmentArgs Args => _args.GetCopy();
	}
}
