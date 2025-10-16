using CloudNimble.EasyAF.Core;
using System;
using System.Diagnostics;

namespace CloudNimble.EasyAF.Restier
{

    /// <summary>
    /// Provides utility methods for logging Restier operations and entity lifecycle events.
    /// Supports logging for both named entities and identifiable entities with detailed operation tracking.
    /// </summary>
    public static class RestierHelpers
    {

        #region Helper Methods

        /// <summary>
        /// Logs a Restier operation for the specified entity type name.
        /// Formats the log message with appropriate verb tense based on operation type.
        /// </summary>
        /// <param name="entityName">The name of the entity type being operated on.</param>
        /// <param name="operation">The type of operation being performed.</param>
        public static void LogOperation(string entityName, RestierOperationType operation)
        {
            Trace.TraceInformation($"{DateTime.Now}: {entityName} {(operation.ToString().EndsWith("ing") ? "is" : "was")} {operation.ToString().ToLower()}.");
        }

        /// <summary>
        /// Logs a Restier operation for the specified DbObservableObject entity.
        /// Extracts the entity type name and delegates to the string-based logging method.
        /// </summary>
        /// <param name="entity">The entity being operated on.</param>
        /// <param name="operation">The type of operation being performed.</param>
        public static void LogOperation(DbObservableObject entity, RestierOperationType operation)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));
            LogOperation(entity.GetType().Name, operation);
        }

        /// <summary>
        /// Logs a Restier operation for the specified identifiable entity, including the entity's ID in the log message.
        /// Provides more detailed logging by including the specific entity identifier.
        /// </summary>
        /// <typeparam name="T">The type of entity that implements IIdentifiable.</typeparam>
        /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
        /// <param name="entity">The identifiable entity being operated on.</param>
        /// <param name="operation">The type of operation being performed.</param>
        public static void LogOperation<T, TId>(T entity, RestierOperationType operation) where T : IIdentifiable<TId> where TId : struct
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));
            Trace.TraceInformation($"{DateTime.Now}: {entity.GetType().Name} '{entity.Id}' {(operation.ToString().EndsWith("ing") ? "is" : "was")} {operation.ToString().ToLower()}.");
        }

        #endregion

    }

}
