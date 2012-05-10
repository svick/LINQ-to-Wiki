namespace LinqToWiki.Internals
{
    /// <summary>
    /// Type of query module.
    /// </summary>
    public enum QueryType
    {
        List,
        Prop,
        Meta
    }

    /// <summary>
    /// Distinguishes what parameter does the <c>dir</c> parameter of a module take:
    /// <c>ascending</c>/<c>descending</c> or <c>newer</c>/<c>older</c>.
    /// </summary>
    public enum SortType
    {
        Ascending,
        Newer
    }
}