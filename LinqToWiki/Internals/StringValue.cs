namespace LinqToWiki.Internals
{
    /// <summary>
    /// Base type for string “enums”.
    /// </summary>
    public abstract class StringValue
    {
        public string Value { get; private set; }

        protected StringValue(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}