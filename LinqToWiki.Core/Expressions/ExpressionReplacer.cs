using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LinqToWiki.Expressions
{
    /// <summary>
    /// Replaces an expression with another one.
    /// </summary>
    public class ExpressionReplacer : ExpressionVisitor
    {
        private readonly Expression m_toReplace;
        private readonly Expression m_replaceWith;

        private ExpressionReplacer(Expression toReplace, Expression replaceWith)
        {
            m_toReplace = toReplace;
            m_replaceWith = replaceWith;
        }

        public override Expression Visit(Expression exp)
        {
            if (exp == m_toReplace)
                return m_replaceWith;

            return base.Visit(exp);
        }

        /// <summary>
        /// Returns <paramref name="expression"/> with instances of <paramref name="toReplace"/>
        /// replaced by <paramref name="replaceWith"/>.
        /// </summary>
        public static Expression Replace(
            Expression expression, Expression toReplace, Expression replaceWith)
        {
            Contract.Ensures(Contract.Result<object>() != null);

            var replacer = new ExpressionReplacer(toReplace, replaceWith);
            return replacer.Visit(expression);
        }
    }
}