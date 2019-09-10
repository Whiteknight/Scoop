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
            "async",
            "await",
            "class",
            "const",
            "delegate",
            "dynamic",
            "enum",
            "false",
            "interface",
            "namespace",
            "new",
            "null",
            "params",
            "partial",
            "private",
            "public",
            "return",
            "struct",
            "throw",
            "true",
            "using",
            "var",
            "where"
        };
    }
}