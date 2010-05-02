using System;
using System.Text;
using StatLight.Client.Harness.Events;

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


        internal static string WriteDebug(this object message)
        {
            var stringBuilder = new StringBuilder();
            var type = message.GetType();
            Action<string> log = msg => stringBuilder.AppendLine(msg);

            var properties = type.GetProperties();

            log(type.Name);
            log("  {");
            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(message, null);
                var msg = "    {0,24}: {1}".FormatWith(propertyInfo.Name, value);
                log(msg);
            }
            log("  }");

            return stringBuilder.ToString();
        }
    }
}
