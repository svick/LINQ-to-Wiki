namespace LinqToWiki.Codegen.ModuleInfo
{
    class SimpleParameterType : ParameterType
    {
        public string Name { get; private set; }

        public SimpleParameterType(string name)
        {
            Name = name;
        }

        public override bool Equals(ParameterType other)
        {
            var otherSimple = other as SimpleParameterType;

            if (otherSimple == null)
                return false;

            return this.Name == otherSimple.Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}