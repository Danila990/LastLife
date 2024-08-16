using System;

namespace Db.Map
{
	[Serializable]
	public class MapData
	{
		public MapObject MapObject;
	}

	public static class UniqueMapObject
	{
		public static readonly string[] PrefKeys = new string[]
		{
			"UniqueItem_3Ticket",
			"UniqueItem_Ticket_Market_Near_Satellite_Dish",
			"Market_Desert_Gates"
		};
		
		
	}
}