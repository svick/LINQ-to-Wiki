using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace LinqToWiki.Expressions
{
    public static class ExpressionFinder
    {
         private class Finder<T> : ExpressionVisitor where T : Expression
         {
             private readonly Func<T, bool> m_condition;

             private readonly List<T> m_results = new List<T>();

             public IEnumerable<T> Results
             {
                 get { return m_results; }
             }

             public Finder(Func<T, bool> condition)
             {
                 m_condition = condition;
             }

             public override Expression Visit(Expression node)
             {
                 var casted = node as T;

                 if (casted != null && m_condition(casted))
                     m_results.Add(casted);

                 return base.Visit(node);
             }
         }

        public static T Single<T>(Expression expression, Func<T, bool> condition) where T : Expression
        {
            var finder = new Finder<T>(condition);
            finder.Visit(expression);
            return finder.Results.Single();
        }
    }
}