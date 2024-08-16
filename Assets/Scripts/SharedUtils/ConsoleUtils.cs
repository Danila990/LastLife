namespace SharedUtils
{
	public static class ConsoleUtils
	{
		public static string SetTextColor(string source, string color = "green")
		{
			return $"<color='{color}'>{source}</color>";
		}
		
		public static string SetColor(this string source, string color = "green")
		{
			return $"<color='{color}'>{source}</color>";
		}
	}
}