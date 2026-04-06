namespace CloudNimble.EasyAF.Core
{
    /// <summary>
    /// Specifies how HttpClient message handlers should be configured when registering HTTP clients.
    /// Determines whether handlers are added to existing handlers or replace them entirely.
    /// </summary>
    public enum HttpHandlerMode
    {

        /// <summary>
        /// No custom message handlers are configured for the HttpClient.
        /// Uses the default handler configuration provided by the HttpClientFactory.
        /// </summary>
        None,

        /// <summary>
        /// Adds custom message handlers to the existing handler pipeline.
        /// Custom handlers are appended to any existing handlers already configured.
        /// </summary>
        Add,

        /// <summary>
        /// Replaces the entire handler pipeline with custom message handlers.
        /// All existing handlers are removed and replaced with the specified custom handlers.
        /// </summary>
        Replace, 

    }

}

