using Core.Player.MovementFSM.Data.Airborne;
using UnityEngine;
using Utils;

namespace Core.Player.MovementFSM.Data
{
	
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(PlayerMovementFsmData), fileName = "PlayerMovementFsmData")]
	public class PlayerMovementFsmData : ScriptableObject
	{
		[field:SerializeField] public PlayerGroundedData PlayerGroundedData { get; private set; }
		[field:SerializeField] public PlayerAirborneData PlayerAirborneData { get; private set; }
	}
}