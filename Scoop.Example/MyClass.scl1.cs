namespace Scoop.Example
{
    public sealed class MyClass 
    {
        public MyClass()
        {
            // Do not allow concrete inheritance
            System.Diagnostics.Debug.Assert(GetType().BaseType == typeof(object));
            
        }
        public void MyMethod()
        {
        }
    }
}
