using Microsoft.Pex.Framework;

namespace LinqToWiki.Expressions
{
    /// <summary>A factory for LinqToWiki.Expressions.UsedPropertiesGatherer instances</summary>
    public static partial class UsedPropertiesGathererFactory
    {
        /// <summary>A factory for LinqToWiki.Expressions.UsedPropertiesGatherer instances</summary>
        [PexFactoryMethod(typeof(IFirst), "LinqToWiki.Expressions.UsedPropertiesGatherer")]
        internal static UsedPropertiesGatherer Create()
        {
            return new UsedPropertiesGatherer();
        }
    }
}
