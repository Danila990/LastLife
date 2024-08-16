using System;
using UnityEngine;

namespace Db.Map
{
	public class ZoneBoundsObject : BoundsObject
	{
		[SerializeField] private ZoneType _zoneType;
		
		public ZoneType ZoneType => _zoneType; 
	}
	
	[Flags]
	public enum ZoneType
	{
		NoGuns = 1 << 0,
		Walkable = 1 << 1,
	}
}
