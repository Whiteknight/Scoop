using ParserObjects;

namespace Scoop
{
    public class Diagnostic
    {
        public string ErrorMessage { get; }
        public Location Location { get; private set; }

        public Diagnostic(string errorMessage, Location location)
        {
            ErrorMessage = errorMessage;
            Location = location;
        }

        public override string ToString()
        {
            return $"{ErrorMessage} at {Location}";
        }

        public Diagnostic AddLocation(Location l)
        {
            Location = Location ?? l;
            return this;
        }
    }
}