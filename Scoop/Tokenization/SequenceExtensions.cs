namespace Scoop.Tokenization
{
    public static class SequenceExtensions
    {
        public static void Advance<T>(this ISequence<T> tokenizer) => tokenizer.GetNext();

        public static ISequence<T> Mark<T>(this ISequence<T> tokenizer) => new WindowTokenizer<T>(tokenizer);
    }
}