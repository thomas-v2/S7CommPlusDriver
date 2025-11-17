using System.Collections.Generic;
using System.Linq;

namespace S7CommPlusDriver.Net
{
    public class Error
    {
        private static readonly Dictionary<int, string> errorsDictionary;

        static Error()
        {
            var fields = typeof(S7Consts).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            errorsDictionary = fields.Where(x => x.Name.StartsWith("err")).ToDictionary(x => (int)x.GetValue(null), x => x.Name);
        }

        public static string GetErrorText(int error)
        {
            if (errorsDictionary.TryGetValue(error, out var text))
                return text;
            return null;
        }
    }
}
