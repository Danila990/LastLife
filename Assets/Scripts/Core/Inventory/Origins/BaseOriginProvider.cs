using Core.Inventory.Items;
using UnityEngine;

namespace Core.Inventory.Origins
{
	public abstract class BaseOriginProvider : MonoBehaviour
	{
		public abstract Transform GetOrigin(string id);
		public abstract Transform GetStaticOrigin();
		public abstract Transform GetOrigin(string aim, ItemContext itemContext);
		public abstract Vector3 GetMeleeOrigin();
	}
}