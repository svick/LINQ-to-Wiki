using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinqToWiki
{
    /// <summary>
    /// Represenst a namespace on a wiki.
    /// See http://www.mediawiki.org/wiki/Manual:Namespace.
    /// </summary>
    public class Namespace : IEquatable<Namespace>
    {
        /// <summary>
        /// Initializes the default namespaces.
        /// </summary>
        static Namespace()
        {
            Article = Create(0, "");
            Talk = Create(1, "Talk");
            User = Create(2, "User");
            UserTalk = Create(3, "User talk");
            Project = Create(4, "Project");
            ProjectTalk = Create(5, "Project talk");
            File = Create(6, "File");
            FileTalk = Create(7, "File talk");
            MediaWiki = Create(8, "MediaWiki");
            MediaWikiTalk = Create(9, "MediaWikiTalk");
            Template = Create(10, "Template");
            TemplateTalk = Create(11, "Template talk");
            Help = Create(12, "Help");
            HelpTalk = Create(13, "Help talk");
            Category = Create(14, "Category");
            CategoryTalk = Create(15, "Category talk");
        }

        /// <summary>
        /// ID of the namespace
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Name of the namespace
        /// </summary>
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

        /// <summary>
        /// Returns a namespace based on its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Namespace Get(int id)
        {
            Namespace result;
            if (!Namespaces.TryGetValue(id, out result))
                result = new Namespace(id, null);

            return result;
        }

        /// <summary>
        /// Parses the <c>namespaces</c> element of a <c>siteinfo</c> query,
        /// returning a collection of namespaces on a wiki.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IEnumerable<Namespace> Parse(XElement element)
        {
            return element.Element("namespaces").Elements()
                .Select(e => new Namespace((int)e.Attribute("id"), (string)e));
        }

        public static Namespace Article { get; private set; }

        public static Namespace Talk { get; private set; }

        public static Namespace User { get; private set; }

        public static Namespace UserTalk { get; private set; }

        public static Namespace Project { get; private set; }

        public static Namespace ProjectTalk { get; private set; }

        public static Namespace File { get; private set; }

        public static Namespace FileTalk { get; private set; }

        public static Namespace MediaWiki { get; private set; }

        public static Namespace MediaWikiTalk { get; private set; }

        public static Namespace Template { get; private set; }

        public static Namespace TemplateTalk { get; private set; }

        public static Namespace Help { get; private set; }

        public static Namespace HelpTalk { get; private set; }

        public static Namespace Category { get; private set; }

        public static Namespace CategoryTalk { get; private set; }

        public static bool operator ==(Namespace first, Namespace second)
        {
            return Equals(first, second);
        }

        public static bool operator !=(Namespace first, Namespace second)
        {
            return !(first == second);
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