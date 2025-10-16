using System;
using System.Diagnostics;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// Provides methods for ensuring that method arguments meet specific criteria.
    /// This class provides a consistent way to validate arguments and throw appropriate exceptions.
    /// </summary>
    /// <example>
    /// <code>
    /// public void ProcessData(string input, List&lt;string&gt; items)
    /// {
    ///     Ensure.ArgumentNotNull(input, nameof(input));
    ///     Ensure.ArgumentNotNull(items, nameof(items));
    ///     
    ///     // Process the validated arguments
    /// }
    /// </code>
    /// </example>
    public static class Ensure
    {

        /// <summary>
        /// Ensures that the specified argument is not null.
        /// </summary>
        /// <param name="argument">The argument to validate.</param>
        /// <param name="argumentName">The name of the argument being validated.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
        [DebuggerStepThrough]
        public static void ArgumentNotNull(object argument, string argumentName)
        {

#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(argument, argumentName);
#else
            if (argument is null)
            {
                throw new ArgumentNullException(argumentName);
            }
#endif

        }

        /// <summary>
        /// Ensures that the specified argument is not null or whitespace.
        /// </summary>
        /// <param name="argument">The argument to validate.</param>
        /// <param name="argumentName">The name of the argument being validated.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="argument"/> is null or whitespace.</exception>
        [DebuggerStepThrough]
        public static void ArgumentNotNullOrWhiteSpace(string argument, string argumentName)
        {

#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNullOrWhiteSpace(argument, argumentName);
#else
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException("Argument cannot be null or whitespace.", argumentName);
            }
#endif

        }

    }

}
