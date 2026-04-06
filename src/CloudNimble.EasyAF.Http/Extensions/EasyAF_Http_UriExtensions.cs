using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace System
{

    /// <summary>
    /// Provides extension methods for Uri objects to support OData query string construction.
    /// Enables fluent API for building OData-compliant URLs with filtering, paging, and sorting capabilities.
    /// </summary>
    public static class EasyAF_Http_UriExtensions
    {

        /// <summary>
        /// Creates an properly-constructed OData Uri with the correct querystring values, if specified.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> instance to extend. </param>
        /// <param name="dollarSign">Specifies whether or not the query string name should have a "$" in it. Defaults to <see langword="true"/>.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="top">An <see cref="int"/> representing the number of records to take.</param>
        /// <param name="skip">An <see cref="int"/> representing the number of records to skip over.</param>
        /// <param name="orderby">The orderby.</param>
        /// <param name="expand">The expand.</param>
        /// <param name="select">The select.</param>
        /// <param name="count">A <see cref="bool"/> representing whether to return a count of the total number of records in the response.</param>
        /// <returns>A new <see cref="Uri"/> instance with a properly-formatted OData-compatible query string.</returns>
        /// <remarks>Inspired by https://github.com/radzenhq/radzen-blazor/blob/master/Radzen.Blazor/OData.cs#L235, but performs better.</remarks>
        public static Uri ToODataUri(this Uri uri, bool dollarSign = true, string filter = null, int? top = null, int? skip = null, string orderby = null, string expand = null, string select = null, bool? count = null)
        {
            var uriBuilder = new UriBuilder(uri);
            var queryString = HttpUtility.ParseQueryString(uriBuilder.Query);

            var queryParameters = new NameValueCollection();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                queryParameters[dollarSign ? "$filter" : "filter"] = filter.Replace("\"", "'");
            }

            if (top is not null)
            {
                queryParameters[dollarSign ? "$top" : "top"] = top.ToString();
            }

            if (skip is not null)
            {
                queryParameters[dollarSign ? "$skip" : "skip"] = skip.ToString();
            }

            if (!string.IsNullOrWhiteSpace(orderby))
            {
                queryParameters[dollarSign ? "$orderby" : "orderby"] = orderby;
            }

            if (!string.IsNullOrWhiteSpace(expand))
            {
                queryParameters[dollarSign ? "$expand" : "expand"] = expand;
            }

            if (!string.IsNullOrWhiteSpace(select))
            {
                queryParameters[dollarSign ? "$select" : "select"] = select;
            }

            if (count is not null)
            {
                queryParameters[dollarSign ? "$count" : "count"] = count.ToString().ToLower();
            }

            var queryBuilder = new StringBuilder();
            foreach (string key in queryParameters)
            {
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Append('&');
                }
                queryBuilder.Append($"{key}={HttpUtility.UrlEncode(queryParameters[key])}");
            }

            uriBuilder.Query = queryBuilder.ToString();

            return uriBuilder.Uri;
        }

    }

}
