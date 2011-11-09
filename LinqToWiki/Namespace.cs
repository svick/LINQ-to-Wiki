namespace LinqToWiki
{
    public class Namespace : IQueryRepresentation
    {
        static Namespace()
        {
            Article = new Namespace(0, "Article", "");
            Talk = new Namespace(1, "Talk");
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Prefix { get; private set; }

        private Namespace(int id, string name, string prefix = null)
        {
            Id = id;
            Name = name;
            Prefix = prefix ?? name;
        }

        public static Namespace Article { get; private set; }

        public static Namespace Talk { get; private set; }

        public string GetQueryRepresentation()
        {
            return Id.ToString();
        }
    }
}