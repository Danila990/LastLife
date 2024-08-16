namespace Core.Entity
{
	public interface IRagdollManager
	{
		void SetState(RagdollState ragdollState);
		void EnableRagDoll();
		void DisableRagDoll();
		void Death();
	}

	public enum RagdollState
	{
		Normal,
		Drag,
		Ragdoll,
	}
}