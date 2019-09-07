namespace Scoop.Example
{
    public class MyClass 
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
