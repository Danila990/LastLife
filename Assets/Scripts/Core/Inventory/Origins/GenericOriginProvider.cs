using System;
using Core.Inventory.Items;
using UnityEngine;

namespace Core.Inventory.Origins
{
	public class GenericOriginProvider : BaseOriginProvider
	{
		public TransformPair[] Origins;
		
		public override Transform GetOrigin(string id)
		{
			foreach (var pair in Origins)
			{
				if (pair.Id == id)
					return pair.Transform;
			}
			return Origins[0].Transform;
		}
		
		public override Transform GetStaticOrigin()
		{
			return Origins[0].Transform;
		}
		
		public override Transform GetOrigin(string aim, ItemContext itemContext)
		{
			return GetOrigin(aim);
		}
		public override Vector3 GetMeleeOrigin()
		{
			return GetOrigin("melee").position;
		}

		[Serializable]
		public struct TransformPair
		{
			public string Id;
			public Transform Transform;
		}
	}
}