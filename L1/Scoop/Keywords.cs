using System.Collections.Generic;

namespace Scoop
{
    public static class Keywords
    {
        public static bool IsKeyword(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            return _keywords.Contains(s);
        }

        public static readonly HashSet<string> _keywords = new HashSet<string>
        {
            // C# Keywords which are still allowed
            // (type names like "int" are counted as types not keywords for these purposes)
            "class",
            "false",
            "interface",
            "namespace",
            "new",
            "private",
            "public",
            "return",
            "true",
            "using"
        };
    }
}