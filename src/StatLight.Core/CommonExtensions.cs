
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StatLight
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;

    public static partial class Extensions
    {
        public static void RaiseEventSafely<T>(this EventHandler handler, object sender, T eventArgs)
            where T : EventArgs
        {
            var handler2 = handler;
            if (handler2 != null)
                handler2(sender, eventArgs);
        }

        public static DateTime ToMidnight(this DateTime dateTime)
        {
            return dateTime.Subtract(dateTime.TimeOfDay);
        }

        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static Stream ToStream(this string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value));
        }

        public static Stream ToStream(this byte[] byteArray)
        {
            return new MemoryStream(byteArray);
        }

        public static IEnumerable<T> Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items == null)
                return null;
            if (action == null) throw new ArgumentNullException("action");

            foreach (var item in items)
                action(item);

            return items;
        }

        public static Collection<T> ToCollection<T>(this IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");
            var collection = new Collection<T>();
            foreach (var item in items)
                collection.Add(item);
            return collection;
        }
    }

    namespace Core.Serialization
    {
        public static partial class Extensions
        {
            public static string StreamToString(this Stream stream)
            {
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            public static T Deserialize<T>(this Stream stream)
            {
                var s = new DataContractSerializer(typeof(T));
                return (T)s.ReadObject(stream);
            }

            public static T Deserialize<T>(this string value)
            {
                return Deserialize<T>(value.ToStream());
            }

            public static string Serialize<T>(this T @object)
            {
                using (var memoryStream = new MemoryStream())
                {
                    var serializer = new DataContractSerializer(@object.GetType());
                    serializer.WriteObject(memoryStream, @object);
                    var bytes = memoryStream.ToArray();
                    return (new UTF8Encoding()).GetString(bytes, 0, bytes.Length);
                }
            }

        }
    }

}
