using System.Linq.Expressions;

namespace LinqToWiki.Expressions
{
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

        public static Expression Replace(
            Expression expression, Expression toReplace, Expression replaceWith)
        {
            var replacer = new ExpressionReplacer(toReplace, replaceWith);
            return replacer.Visit(expression);
        }
    }
}