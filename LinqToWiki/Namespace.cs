using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace LinqToWiki
{
    public class Namespace : IQueryRepresentation, IEquatable<Namespace>
    {
        static Namespace()
        {
            Article = Create(0, "");
            Talk = Create(1, "Talk");
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        private static readonly Dictionary<int, Namespace> Namespaces = new Dictionary<int, Namespace>();

        private static Namespace Create(int id, string name)
        {
            var result = new Namespace(id, name);
            Namespaces.Add(id, result);
            return result;
        }

        private Namespace(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public static Namespace Get(int id)
        {
            Namespace result;
            if (!Namespaces.TryGetValue(id, out result))
                result = new Namespace(id, null);

            return result;
        }

        public static Namespace Parse(XElement element)
        {
            return new Namespace((int)element.Attribute("id"), (string)element);
        }

        public static Namespace Article { get; private set; }

        public static Namespace Talk { get; private set; }

        public string GetQueryRepresentation()
        {
            return Id.ToString(CultureInfo.InvariantCulture);
        }

        public bool Equals(Namespace other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Namespace))
                return false;
            return Equals((Namespace)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Name: '{1}'", Id, Name);
        }
    }
}