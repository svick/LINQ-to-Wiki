using System;
using System.Linq;
using System.Linq.Expressions;
using IQToolkit;
using LinqToWiki.Internals;
using LinqToWiki.Parameters;

namespace LinqToWiki.Expressions
{
    /// <summary>
    /// Parses various expression kinds and returns them as a <see cref="QueryParameters{TSource,TResult}"/>.
    /// </summary>
    static class ExpressionParser
    {
        /// <summary>
        /// Parses a <c>where</c> expression.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <param name="previousParameters">Previous parameters, whose values should be included in the result.</param>
        public static QueryParameters<TSource, TResult> ParseWhere<TSource, TResult, TWhere>(
            Expression<Func<TWhere, bool>> expression, QueryParameters<TSource, TResult> previousParameters)
        {
            var body = EnumFixer.Fix(PartialEvaluator.Eval(expression.Body));

            return ParseWhereSubexpression(body, previousParameters);
        }

        /// <summary>
        /// Parses a single expression from <c>where</c>. If necessary, calls itself recursivelly.
        /// </summary>
        private static QueryParameters<TSource, TResult> ParseWhereSubexpression<TSource, TResult>(
            Expression body, QueryParameters<TSource, TResult> previousParameters)
        {
            var memberExpression = body as MemberExpression;

            if (memberExpression != null)
                return AddValue(previousParameters, memberExpression, true);

            var unaryExpression = body as UnaryExpression;

            if (unaryExpression != null)
            {
                var memberAccess = unaryExpression.Operand as MemberExpression;
                if (unaryExpression.NodeType == ExpressionType.Not && memberAccess != null)
                    return AddValue(previousParameters, memberAccess, false);

                throw new ArgumentException(string.Format("Unknown type of unary expression: {0}.", unaryExpression));
            }

            var binaryExpression = body as BinaryExpression;

            if (binaryExpression != null)
            {
                if (binaryExpression.NodeType == ExpressionType.Equal)
                    return AddValue(previousParameters, ParseWhereEqualExpression(binaryExpression));

                if (binaryExpression.NodeType == ExpressionType.AndAlso || binaryExpression.NodeType == ExpressionType.And)
                {
                    var afterLeft = ParseWhereSubexpression(binaryExpression.Left, previousParameters);
                    return ParseWhereSubexpression(binaryExpression.Right, afterLeft);
                }

                throw new ArgumentException(string.Format("Unknown type of binary expression: {0}.", binaryExpression));
            }

            throw new ArgumentException(string.Format("Unknown type of expression: {0}.", body));
        }

        /// <summary>
        /// Adds single value to QueryParameters based on a property expression and a unformatted value from a <c>Tuple</c>.
        /// </summary>
        private static QueryParameters<TSource, TResult> AddValue<TSource, TResult>(
            QueryParameters<TSource, TResult> previousParameters, Tuple<MemberExpression, object> propertyValue)
        {
            return AddValue(previousParameters, propertyValue.Item1, propertyValue.Item2);
        }

        /// <summary>
        /// Adds single value to QueryParameters based on a property expression and a unformatted value.
        /// </summary>
        private static QueryParameters<TSource, TResult> AddValue<TSource, TResult>(
            QueryParameters<TSource, TResult> previousParameters, MemberExpression memberExpression, object value)
        {
            return previousParameters.AddSingleValue(
                ReversePropertyName(memberExpression.Member.Name.ToLowerInvariant()),
                QueryRepresentation.ToQueryStringDynamic(value));
        }

        /// <summary>
        /// Parses a part of <c>where</c> expression that contains <c>==</c>.
        /// </summary>
        /// <param name="expression">Subexpression to parse.</param>
        private static Tuple<MemberExpression, object> ParseWhereEqualExpression(BinaryExpression expression)
        {
            var result = ParsePropertyEqualsConstantExpression(expression)
                         ?? ParsePropertyEqualsConstantExpression(expression.Switch());

            if (result == null)
                throw new ArgumentException(string.Format("Could not parse expression: {0}.", expression));

            return result;
        }

        /// <summary>
        /// Parses an expression that contains <c>==</c> and parameters in the “correct” order.
        /// </summary>
        private static Tuple<MemberExpression, object> ParsePropertyEqualsConstantExpression(BinaryExpression expression)
        {
            var memberAccess = expression.Left as MemberExpression;

            if (memberAccess == null)
                return null;

            if (!(memberAccess.Expression is ParameterExpression))
                return null;

            var valueExpression = expression.Right as ConstantExpression;

            if (valueExpression == null)
                return null;

            object value = valueExpression.Value;

            return Tuple.Create(memberAccess, value);
        }

        /// <summary>
        /// Reverses property name into the form used by the API.
        /// </summary>
        public static string ReversePropertyName(string propertyName)
        {
            if (propertyName == "value")
                return "*";
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

        /// <summary>
        /// Parses a <c>select</c> expression that looks like an identity (based on it type).
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <param name="previousParameters">Previous parameters, whose values should be included in the result.</param>
        public static QueryParameters<T, T> ParseIdentitySelect<T>(Expression<Func<T, T>> expression, QueryParameters<T, T> previousParameters)
        {
            var parameter = expression.Parameters.Single();

            var body = expression.Body as ParameterExpression;

            if (parameter != body)
                throw new InvalidOperationException(
                    string.Format(
                        "Select expression with the return type of '{0}' has to be identity.", expression.Body.Type));

            return previousParameters;
        }
    }
}