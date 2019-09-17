namespace Scoop
{
    public static class Errors
    {
        // Class to hold error messages. The only error messages we should be matching against
        // in our test suite should be here. Otherwise we shouldn't be relying on the exact
        // text of any error message

        public const string MissingNamespaceName = "Missing namespace name";

        public const string MissingCloseAngle = "Missing >";

        public const string MissingCloseBrace = "Missing ]";

        public const string MissingOpenBracket = "Missing {";
        public const string MissingCloseBracket = "Missing }";

        public const string MissingEquals = "Missing =";

        public const string MissingExpression = "Missing expression";

        public const string MissingOpenParen = "Missing (";
        public const string MissingCloseParen = "Missing )";

        public const string MissingColon = "Missing :";
        public const string MissingSemicolon = "Missing ;";

        public const string MissingParameterList = "Missing parameter list";

        public const string MissingIdentifier = "Missing identifier";

        public const string MissingStatement = "Missing statement";

        public const string MissingThis = "Missing this";

        public const string MissingType = "Missing type";

        public const string UnexpectedToken = "Unexpected token";
    }
}
