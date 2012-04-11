using System;
using System.Linq;
using System.Linq.Expressions;
using IQToolkit;
using LinqToWiki.Parameters;

namespace LinqToWiki.Expressions
{
    /// <summary>
    /// Parses various expression kinds and returns them as a <see cref="QueryParameters{TSource,TResult}"/>.
    /// </summary>
    public static class ExpressionParser
    {
        /// <summary>
        /// Parses a <c>where</c> expression.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <param name="previousParameters">Previous parameters, whose values should be included in the result.</param>
        public static QueryParameters<TSource, TResult> ParseWhere<TSource, TResult, TWhere>(
            Expression<Func<TWhere, bool>> expression, QueryParameters<TSource, TResult> previousParameters)
        {
            // TODO: parse more complicated expressions, like Contains()

            var body = EnumFixer.Fix(PartialEvaluator.Eval(expression.Body));

            var binaryExpression = body as BinaryExpression;

            if (binaryExpression != null)
            {
                if (binaryExpression.NodeType == ExpressionType.Equal)
                    return ParseWhereEqualExpression(binaryExpression, previousParameters);
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Parses a part of <c>where</c> expression that contains <c>==</c>.
        /// </summary>
        /// <param name="expression">Subexpression to parse.</param>
        /// <param name="previousParameters">Previous parameters, whose values should be included in the result.</param>
        private static QueryParameters<TSource, TResult> ParseWhereEqualExpression<TSource, TResult>(
            BinaryExpression expression, QueryParameters<TSource, TResult> previousParameters)
        {
            // TODO: handle reverse order

            var memberAccess = expression.Left as MemberExpression;

            if (memberAccess == null)
                throw new ArgumentException();

            if (!(memberAccess.Expression is ParameterExpression))
                throw new ArgumentException();

            string propertyName = memberAccess.Member.Name.ToLowerInvariant();

            var valueExpression = expression.Right as ConstantExpression;

            if (valueExpression == null)
                throw new ArgumentException();

            object value = valueExpression.Value;
            var valueQueryRepresentation = value as IQueryRepresentation;

            string valueString = valueQueryRepresentation != null
                                     ? valueQueryRepresentation.GetQueryRepresentation()
                                     : value.ToString();

            return previousParameters.AddSingleValue(ReversePropertyName(propertyName), valueString);
        }

        public static string ReversePropertyName(string propertyName)
        {
            if (propertyName == "value")
                return "*";
            if (propertyName == "ns")
                return "namespace";
            if (propertyName == "defaultvalue")
                return "default";

            return propertyName;
        }

        /// <summary>
        /// Parses an <c>orderby</c> expression.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <param name="previousParameters">Previous parameters, whose values should be included in the result.</param>
        /// <param name="ascending">Should the ordering be ascending?</param>
        public static QueryParameters<TSource, TResult> ParseOrderBy<TSource, TResult, TOrderBy, TKey>(
            Expression<Func<TOrderBy, TKey>> expression,
            QueryParameters<TSource, TResult> previousParameters,
            bool ascending)
        {
            var parameter = expression.Body as ParameterExpression;
            var memberAccess = expression.Body as MemberExpression;

            string memberName;
            if (parameter != null)
                memberName = null;
            else if (memberAccess != null)
            {
                if (!(memberAccess.Expression is ParameterExpression))
                    throw new ArgumentException();

                memberName = memberAccess.Member.Name.ToLowerInvariant();
            }
            else
                throw new ArgumentException();

            return previousParameters.WithSort(memberName, ascending);
        }

        /// <summary>
        /// Parses a <c>select</c> expression.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <param name="previousParameters">Previous parameters, whose values should be included in the result.</param>
        public static QueryParameters<TSource, TResult> ParseSelect<TSource, TResult>(
            Expression<Func<TSource, TResult>> expression, QueryParameters<TSource, TSource> previousParameters)
        {
            var parameter = expression.Parameters.Single();

            var gatherer = new UsedPropertiesGatherer();

            gatherer.Gather(expression.Body, parameter);

            var usedProperties = gatherer.UsedDirectly ? null : gatherer.UsedProperties.Select(p => p.ToLowerInvariant());

            return previousParameters.WithSelect(usedProperties, expression.Compile());
        }
    }
}