namespace Scoop
{
    public class Location
    {
        public Location(string fileName, int line, int column)
        {
            FileName = fileName;
            Line = line;
            Column = column;
        }

        public string FileName { get; set; }
        public int Line { get; }
        public int Column { get; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FileName))
                return $"File {FileName} at Line {Line} Column {Column}";
            return $"Line {Line} Column {Column}";
        }
    }
}
