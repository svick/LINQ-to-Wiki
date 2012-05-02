using LinqToWiki.Internals;

namespace LinqToWiki
{
    public abstract class PagesSource<TPage> : IPagesSource
    {
        private readonly QueryPageProcessor m_queryPageProcessor;

        protected PagesSource(QueryPageProcessor queryPageProcessor)
        {
            m_queryPageProcessor = queryPageProcessor;
        }

        IPagesCollection IPagesSource.GetPagesCollection()
        {
            return GetPagesCollectionInternal();
        }

        protected abstract IPagesCollection GetPagesCollectionInternal();

        QueryPageProcessor IPagesSource.QueryPageProcessor
        {
            get { return m_queryPageProcessor; }
        }
    }
}