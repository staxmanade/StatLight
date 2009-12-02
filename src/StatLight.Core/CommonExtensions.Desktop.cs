namespace StatLight
{
	public static partial class Extensions
	{
		private static bool _initialColorSaved = false;
		private static System.ConsoleColor _initialColor;
		public static void WrapConsoleMessageWithColor(this string message, System.ConsoleColor color, bool useNewLine)
		{
			if (!_initialColorSaved)
			{
				_initialColorSaved = true;
				_initialColor = System.Console.ForegroundColor;
			}
			System.Console.ForegroundColor = color;
			if (useNewLine)
				System.Console.WriteLine(message);
			else
				System.Console.Write(message);
			System.Console.ForegroundColor = _initialColor;
		}
	}
}
