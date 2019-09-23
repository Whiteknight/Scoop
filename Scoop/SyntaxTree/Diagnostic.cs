namespace Scoop.SyntaxTree
{
    public class Diagnostic
    {
        public string ErrorMessage { get; }
        public Location Location { get; }

        public Diagnostic(string errorMessage, Location location)
        {
            ErrorMessage = errorMessage;
            Location = location;
        }

        public override string ToString()
        {
            return $"{ErrorMessage} at {Location}";
        }
    }
}