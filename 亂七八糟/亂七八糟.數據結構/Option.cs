//Option monad
namespace 亂七八糟.數據結構
{
    public class Option<T>
    {
        protected readonly T? _value;
        protected readonly bool _hasValue;
        protected Option(T? value)
        {
            _value = value;
            _hasValue = true;
        }
        protected Option()
        {
            _hasValue = false;
        }
        public static Option<T> Some(T? value) => new Option<T>(value);
        public static Option<T> None() => new Option<T>();
        public bool HasValue => _hasValue;
        public T? Value
        {
            get
            {
                if (!_hasValue)
                {
                    return default;
                }
                return _value;
            }
        }
    }

    public class Option : Option<Object>
    {
        protected Option(object? value) : base(value)
        {
        }
        protected Option() : base()
        {
        }
        public new static Option Some(object? value) => new Option(value);
        public new static Option None() => new Option();
    }
}