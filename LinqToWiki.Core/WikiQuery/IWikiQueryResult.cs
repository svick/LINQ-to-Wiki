using System.Diagnostics.Contracts;
using LinqToWiki.Parameters;

namespace LinqToWiki
{
    /// <summary>
    /// Non-generic base interface for <see cref="WikiQueryResult{TSource,TResult}"/>.
    /// </summary>
    [ContractClass(typeof(WikiQueryResultContracts))]
    interface IWikiQueryResult
    {
        /// <summary>
        /// Parameters that can be used to execute the query.
        /// </summary>
        QueryParameters Parameters { get; }
    }

    [ContractClassFor(typeof(IWikiQueryResult))]
    abstract class WikiQueryResultContracts : IWikiQueryResult
    {
        public QueryParameters Parameters
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);

                return null;
            }
        }
    }
}