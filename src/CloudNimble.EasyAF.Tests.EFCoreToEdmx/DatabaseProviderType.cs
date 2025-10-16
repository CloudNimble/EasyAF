namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{
    /// <summary>
    /// Specifies the database provider type for EDMX generation.
    /// </summary>
    public enum DatabaseProviderType
    {
        /// <summary>
        /// Unknown provider type, requires detection.
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Microsoft SQL Server provider.
        /// </summary>
        SqlServer = 1,
        
        /// <summary>
        /// PostgreSQL provider.
        /// </summary>
        PostgreSQL = 2
    }
}
