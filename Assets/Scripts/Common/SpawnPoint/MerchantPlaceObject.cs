using Core.Entity.Ai.Merchant;
using UnityEngine;

namespace Common.SpawnPoint
{
	public class MerchantPlaceObject : MonoBehaviour
	{
		[field:SerializeField] public string Id { get; private set; }
		public virtual void ConnectedContext(MerchantEntityContext context)
		{
		}
	}
}