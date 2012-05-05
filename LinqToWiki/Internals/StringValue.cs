using System;

namespace LinqToWiki.Internals
{
    /// <summary>
    /// Base type for string “enums”.
    /// </summary>
    public abstract class StringValue : IEquatable<StringValue>
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

        public bool Equals(StringValue other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Value, Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((StringValue)obj);
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        public static bool operator ==(StringValue left, StringValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StringValue left, StringValue right)
        {
            return !Equals(left, right);
        }
    }
}