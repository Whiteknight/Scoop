namespace Scoop.Tokenization
{
    public interface ISequence<T>
    {
        void PutBack(T token);
        T GetNext();

        T Peek();
        Location CurrentLocation { get; }
    }
}