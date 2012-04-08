using System.Xml.Linq;

namespace LinqToWiki
{
    public class CategoryMembersSelect
    {
        public long PageId { get; private set; }

        public string Title { get; private set; }

        public string SortKey { get; private set; }

        public static CategoryMembersSelect Parse(XElement element)
        {
            var result = new CategoryMembersSelect();
            var pageId = element.Attribute("pageid");
            if (pageId != null)
                result.PageId = (long)pageId;
            var title = element.Attribute("title");
            if (title != null)
                result.Title = (string)title;
            var sortKey = element.Attribute("sortkey");
            if (sortKey != null)
                result.SortKey = (string)sortKey;
            return result;
        }
    }
}