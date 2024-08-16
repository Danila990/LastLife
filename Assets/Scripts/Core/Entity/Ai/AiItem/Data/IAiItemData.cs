namespace Core.Entity.Ai.AiItem.Data
{
	public interface IAiItemData
	{
		public float AttackRange { get; set; }
		public bool SelfEnd { get; }
		public float Cooldown { get; set; }
		public float ConstPriority { get; set; }
		public float UseItemDuration { get; }
		public float Damage { get; }
		public bool UseRig { get; }
		public string RigName { get; }
	}
}