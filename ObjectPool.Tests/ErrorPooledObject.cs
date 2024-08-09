namespace ObjectPool.Tests
{
    internal class ErrorPooledObject
    {
        private static int _counter = 0;
        public ErrorPooledObject() 
        {          
            if (_counter++ == 0)
            {
                throw new Exception();
            }            
        }
    }
}
