namespace Larva.DynamicProxy
{
    public class WrapperObject
    {
        private object _val;

        public WrapperObject() { }

        public object Value
        {
            get { return _val; }
            set
            {
                _val = value;
                HasValue = true;
            }
        }

        public bool HasValue { get; private set; }
    }
}
