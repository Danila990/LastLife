namespace Core.Quests.Priority
{
	public readonly struct ModifiedPriorityArgs
	{
		public readonly string InlineId;
		public readonly int ModifiedPriority;
		public readonly int InitialPriority;
		public ModifiedPriorityArgs(string inlineId, int initialPriority, int modifiedPriority)
		{
			InlineId = inlineId;
			InitialPriority = initialPriority;
			ModifiedPriority = modifiedPriority;
		}
	}
}
