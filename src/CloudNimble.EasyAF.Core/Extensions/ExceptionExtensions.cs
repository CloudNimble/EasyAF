using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{

    /// <summary>
    /// 
    /// </summary>
    public static class EasyAF_ExceptionExtensions
    {

        /// <summary>
        /// Demystifies the Exception and writes it to <see cref="Trace.TraceError(string, object[])"/>.
        /// </summary>
        /// <param name="ex">The exception instance to manipulate.</param>
        /// <param name="logPrefix">A string that will be prepended to the log entry. Defaults to the calling function name.</param>
        /// <returns>The Demystified exception.</returns>
        public static Exception TraceDemystifiedException(this Exception ex, [CallerMemberName]string logPrefix = "")
        {
            var exception = ex.Demystify();
            Trace.TraceError("{0}: Message: {1}, InnerMessage: {2}/nStackTrace: {3}", logPrefix, exception.Message, exception.InnerException?.Message, exception.StackTrace);
            return exception;
        }

    }

}
