using UnityEngine;

namespace Utils.Constants
{
	public static class AHash
	{
		public static readonly int Walk = Animator.StringToHash("Walk");
		public static readonly int Jump = Animator.StringToHash("Jump");
		public static readonly int Idle = Animator.StringToHash("Idle");
		public static readonly int IsMoving = Animator.StringToHash("IsMoving");
		public static readonly int HeavyAttack = Animator.StringToHash("HeavyAttack");
		public static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");
		public static readonly int Death = Animator.StringToHash("Death");
		public static readonly int Dance = Animator.StringToHash("Dance");
		public static readonly int Alarm = Animator.StringToHash("Alarm");
		
		public static readonly int Crawl = Animator.StringToHash("Crawl");
		public static readonly int Impact = Animator.StringToHash("Impact");
		public static readonly int Shocked = Animator.StringToHash("IsShocked");
		public static readonly int Fire = Animator.StringToHash("IsFire");
		public static readonly int Died = Animator.StringToHash("Died");
		public static readonly int Dead = Animator.StringToHash("Dead");
		public static readonly int Damage = Animator.StringToHash("GetDamage");
		public static readonly int CustomOnce = Animator.StringToHash("CustomOnce");
		public static readonly int CustomLoop = Animator.StringToHash("CustomLoop");
		public static readonly int Eating = Animator.StringToHash("Eating");
		public static readonly int Empty = Animator.StringToHash("Empty");
		public static readonly int Used = Animator.StringToHash("Used");
		
		// Reload
		public static readonly int IsReloading = Animator.StringToHash("IsReloading");
		public static readonly int StartReload = Animator.StringToHash("StartReload");
		public static readonly int Reload = Animator.StringToHash("Reload");
		public static readonly int EndReload = Animator.StringToHash("EndReload");
		public static readonly int ReloadMultiplier = Animator.StringToHash("ReloadMultiplier");
		
		public static readonly int Sitting = Animator.StringToHash("Sitting");
		public static readonly int ActionMultiplier = Animator.StringToHash("ActionMultiplier");

		
		public static readonly int StableImpact = Animator.StringToHash("StableImpact");
		
		public static readonly int Climb = Animator.StringToHash("Climb");
		public static readonly int SpeedBoost = Animator.StringToHash("SpeedBoost");
		
		//Head
		public static readonly int MouseOpen = Animator.StringToHash("MouseOpen");
		public static readonly int Hide = Animator.StringToHash("Hide");
		public static readonly int Hello = Animator.StringToHash("Hello");
		public static readonly int HeavyImpact = Animator.StringToHash("HeavyImpact");
		public static readonly int Punch = Animator.StringToHash("Punch");
		public static readonly int Stun = Animator.StringToHash("InStun");

		public static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
		public static readonly int Speed = Animator.StringToHash("Speed");
		public static readonly int DanceSpeed = Animator.StringToHash("DanceSpeed");
		public static readonly int StunSpeed = Animator.StringToHash("StunSpeed");
		public static readonly int Grounded = Animator.StringToHash("Grounded");
		public static readonly int InAir = Animator.StringToHash("InAir");
		public static readonly int Rotate = Animator.StringToHash("Rotate");
		public static readonly int Zipline = Animator.StringToHash("Zipline");
		
		// FSM 
		public static readonly int GroundedParameterHash = Animator.StringToHash("Grounded");
		public static readonly int MovingParameterHash = Animator.StringToHash("Moving");
		public static readonly int StoppingParameterHash = Animator.StringToHash("Stopping");
		public static readonly int LandingParameterHash = Animator.StringToHash("Landing");
		public static readonly int AirborneParameterHash = Animator.StringToHash("Airborne");

		public static readonly int IdleParameterHash = Animator.StringToHash("isIdling");
		public static readonly int DashParameterHash = Animator.StringToHash("isDashing");
		public static readonly int WalkParameterHash = Animator.StringToHash("isWalking");
		public static readonly int RunParameterHash = Animator.StringToHash("isRunning");
		public static readonly int SprintParameterHash = Animator.StringToHash("isSprinting");
		public static readonly int MediumStopParameterHash = Animator.StringToHash("isMediumStopping");
		public static readonly int HardStopParameterHash = Animator.StringToHash("isHardStopping");
		public static readonly int RollParameterHash = Animator.StringToHash("isRolling");
		public static readonly int HardLandParameterHash = Animator.StringToHash("isHardLanding");

		public static readonly int FallParameterHash = Animator.StringToHash("isFalling");
		//public static readonly int IsHardFallingParameterHash = Animator.StringToHash("isHardFalling");
		public static readonly int PlayerVelocityX = Animator.StringToHash("playerVelocityX");
		public static readonly int PlayerVelocityY = Animator.StringToHash("playerVelocityY");
		
		public static readonly int IsMeleeAttack = Animator.StringToHash("isMeleeAttack");
	}

	public static class ShHash
	{
		public static readonly int MainTex = Shader.PropertyToID("_MainTex");
		public static readonly int Shift = Shader.PropertyToID("_Shift");
		public static readonly int DissolveSlider = Shader.PropertyToID("_DissolveSlider");
		public static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
		public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
		public static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
		public static readonly int Emission = Shader.PropertyToID("_Emission");
		public static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
		public static readonly int BlendingWeight = Shader.PropertyToID("_BlendingWeight");
		public static readonly int Metallic = Shader.PropertyToID("_Metallic");
		public static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
		public static readonly int SmoothnessGlossMapScale = Shader.PropertyToID("_GlossMapScale");
		public static readonly int AlphaClipProp = Shader.PropertyToID("_AlphaClipProp");
		public static readonly int MaskPower = Shader.PropertyToID("_MaskPower");
		public static readonly int VignettePower = Shader.PropertyToID("_Vingette_Power");
		public static readonly int Flash = Shader.PropertyToID("_FlashAlpha");
		public static readonly int NoiseSpeed_FullScreen1 = Shader.PropertyToID("_speed");
	}
}