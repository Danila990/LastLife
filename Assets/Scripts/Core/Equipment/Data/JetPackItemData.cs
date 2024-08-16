using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Equipment.Data
{
	[Serializable]
	public class JetPackItemData : EquipmentItemData
	{
		[SerializeField]
		[InlineProperty]
		[HideLabel]
		[BoxGroup("Args")]
		private JetPackItemArgs _args;
		public override IEquipmentArgs Args => _args.GetCopy();
	}

}
