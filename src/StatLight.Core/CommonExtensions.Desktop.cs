using System;
using System.IO;
using System.Text;

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

        public static byte[] ToByteArray(this string value)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(value);
        }

        public static string ToStringFromByteArray(this byte[] value)
        {
            var enc = new UTF8Encoding();
            return enc.GetString(value);
        }

        public static string Hash(this byte[] value)
        {
            var encryptedString = new StringBuilder();
            using (var sha = new System.Security.Cryptography.SHA1Managed())
            {
                var result = sha.ComputeHash(value);
                foreach (byte outputByte in result)
                {
                    // convert each byte to a Hexadecimal upper case string
                    encryptedString.Append(outputByte.ToString("x2").ToUpper());
                }
                return encryptedString.ToString();
            }
        }

    }

    public static class ZipExtensions
    {
        public static byte[] ToByteArray(this Ionic.Zip.ZipFile zip)
        {
            if (zip == null) throw new ArgumentNullException("zip");
            using (var stream = new MemoryStream())
            {
                zip.Save(stream);
                return stream.ToArray();
            }
        }
    }

}
