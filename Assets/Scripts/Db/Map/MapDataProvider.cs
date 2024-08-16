using UnityEngine;
using Utils;

namespace Db.Map
{
	[CreateAssetMenu(menuName = SoNames.SETTINGS + "MapDataProvider", fileName = "MapDataProvider")]
	public class MapDataProvider : ScriptableObject, IMapDataProvider
	{
		[field:SerializeField] public MapData MapData { get; set; }
	}

	public interface IMapDataProvider
	{
		public MapData MapData { get; }
	}
}