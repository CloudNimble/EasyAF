using System;
using System.Linq.Expressions;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// Fills a gap in <see langword="nameof" /> by allowing you to use deep name references instead of local name references.
    /// </summary>
    /// <remarks>
    /// Solution modified from <see href="https://stackoverflow.com/a/58190566/403765" />.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
    public static class NameOf
    {

        #region Public Methods

        /// <summary>
        /// Gets the full property path name from the specified expression, optionally using a custom separator.
        /// </summary>
        /// <typeparam name="TSource">The source type containing the property.</typeparam>
        /// <param name="expression">An expression pointing to the property whose full name should be returned.</param>
        /// <param name="separator">The character(s) used to separate property names in the result. Defaults to ".".</param>
        /// <returns>The full property path as a string with the specified separator.</returns>
        public static string Full<TSource>(Expression<Func<TSource, object>> expression, string separator = ".")
        {
            Ensure.ArgumentNotNull(expression, nameof(expression));

            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression is null)
            {
                if (expression.Body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
                    memberExpression = unaryExpression.Operand as MemberExpression;
            }

            var result = memberExpression.ToString();
#if NET8_0_OR_GREATER
            result = result[(result.IndexOf('.') + 1)..];
#else
            result = result.Substring(result.IndexOf('.') + 1);
#endif

            return separator == "." ? result : result.Replace(".", separator);
        }

        /// <summary>
        /// Allows you to create a source name expression when you need to have a prefixing variable in the result.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="sourceFieldName"></param>
        /// <param name="expression"></param>
        /// <param name="separator">The characters used to separate the from the result.</param>
        /// <returns></returns>
        public static string Full<TSource>(string sourceFieldName, Expression<Func<TSource, object>> expression, string separator = ".")
        {
            var result = Full(expression, separator);
            result = string.IsNullOrEmpty(sourceFieldName) ? result : sourceFieldName + separator + result;
            return result;
        }

#endregion
    }

}
