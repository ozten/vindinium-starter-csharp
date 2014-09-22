namespace Vindinium.Tests
{
    using System.IO;
    using System.Reflection;

    internal static class Util
    {
        internal static string GetResource(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Vindinium.Tests." + resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var outp = reader.ReadToEnd();
                return outp;
            }
        }
    }
}