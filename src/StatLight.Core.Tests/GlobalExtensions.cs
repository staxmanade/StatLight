namespace StatLight.Core.Tests
{
    public static class GlobalExtensions
    {
         public static T Trace<T>(this T item)
         {
             System.Diagnostics.Trace.WriteLine(item);
             return item;
         }
    }
}