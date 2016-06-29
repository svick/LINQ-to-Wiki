using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToWiki.Expressions
{
    /// <summary>
    /// Gathers information about properties of an object used in an expression.
    /// </summary>
    class UsedPropertiesGatherer : ExpressionVisitor
    {
        private Expression m_needle;
        private HashSet<string> m_usedProperties;

        /// <summary>
        /// List of properties of an object used in the expression
        /// </summary>
        public IEnumerable<string> UsedProperties
        {
            get { return m_usedProperties; }
        }

        /// <summary>
        /// Was the object used directly (without accesing its property) in the expression?
        /// </summary>
        public bool UsedDirectly { get; private set; }

        /// <summary>
        /// Gathers information about properties of <paramref name="needle"/> in <paramref name="haystack"/>.
        /// </summary>
        public void Gather(Expression haystack, Expression needle)
        {
            m_usedProperties = new HashSet<string>();

            m_needle = needle;

            Visit(haystack);
        }

        public override Expression Visit(Expression node)
        {
            if (node == m_needle)
            {
                UsedDirectly = true;
                return node;
            }

            return base.Visit(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == m_needle)
            {
                m_usedProperties.Add(node.Member.Name);
                return node;
            }

            return base.VisitMember(node);
        }
    }
}