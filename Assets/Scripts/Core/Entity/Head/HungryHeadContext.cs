using UnityEngine;

namespace Core.Entity.Head
{
	public class HungryHeadContext : HeadContext
	{
		[SerializeField] private Renderer _tongue;
			
		protected override void OnDeath()
		{
			_tongue.enabled = false;
		}
	}
}