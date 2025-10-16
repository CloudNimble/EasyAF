using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace CloudNimble.EasyAF.EFCoreToEdmx.PostgreSQL
{
    /// <summary>
    /// Custom design-time services for PostgreSQL that provides enhanced type mapping
    /// during EF Core reverse engineering (scaffolding) operations.
    /// </summary>
    /// <remarks>
    /// This service replaces the default PostgreSQL type mapping source with our
    /// custom implementation that correctly maps timestamp with time zone columns
    /// to DateTimeOffset CLR types.
    /// </remarks>
    public class PostgreSQLDesignTimeServices : IDesignTimeServices
    {
        /// <summary>
        /// Configures the design-time services for PostgreSQL scaffolding operations.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <remarks>
        /// Registers our custom PostgreSQL type mapping source to replace the default
        /// implementation, ensuring proper type mapping during reverse engineering.
        /// </remarks>
        public void ConfigureDesignTimeServices(IServiceCollection services)
        {
            Console.WriteLine("Registering custom PostgreSQL type mapping source for timestamptz -> DateTimeOffset mapping");
            
            try
            {
                // Find and replace the relational type mapping source
                var existingDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IRelationalTypeMappingSource));
                if (existingDescriptor != null)
                {
                    Console.WriteLine($"Found existing type mapping source: {existingDescriptor.ImplementationType?.Name}");
                    services.Remove(existingDescriptor);
                    
                    // Add our custom type mapping source that wraps the original
                    services.AddSingleton<IRelationalTypeMappingSource>(provider =>
                    {
                        try
                        {
                            // Get the required dependencies for the base class
                            var dependencies = provider.GetService<TypeMappingSourceDependencies>();
                            var relationalDependencies = provider.GetService<RelationalTypeMappingSourceDependencies>();
                            
                            if (dependencies == null)
                            {
                                Console.WriteLine("Warning: TypeMappingSourceDependencies not available, falling back to original");
                                return (IRelationalTypeMappingSource)ActivatorUtilities.CreateInstance(
                                    provider, existingDescriptor.ImplementationType);
                            }
                            
                            if (relationalDependencies == null)
                            {
                                Console.WriteLine("Warning: RelationalTypeMappingSourceDependencies not available, falling back to original");
                                return (IRelationalTypeMappingSource)ActivatorUtilities.CreateInstance(
                                    provider, existingDescriptor.ImplementationType);
                            }
                            
                            // Create the original type mapping source using ActivatorUtilities for proper DI
                            var originalSource = (IRelationalTypeMappingSource)
                                Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance(
                                    provider, existingDescriptor.ImplementationType);
                            
                            if (originalSource == null)
                            {
                                Console.WriteLine("Error: Failed to create original type mapping source");
                                throw new InvalidOperationException("Failed to create original PostgreSQL type mapping source");
                            }
                            
                            Console.WriteLine("Successfully created custom PostgreSQL type mapping source");
                            // Wrap it with our custom source
                            return new PostgreSQLRelationalTypeMappingSource(dependencies, relationalDependencies, originalSource);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating custom type mapping source: {ex.Message}");
                            Console.WriteLine($"Stack trace: {ex.StackTrace}");
                            
                            // Fallback to original implementation if our custom one fails
                            return (IRelationalTypeMappingSource)ActivatorUtilities.CreateInstance(
                                provider, existingDescriptor.ImplementationType);
                        }
                    });
                }
                else
                {
                    Console.WriteLine("Warning: No existing IRelationalTypeMappingSource found to replace");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConfigureDesignTimeServices: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Don't throw - let the original services continue to work
            }
        }
    }
}
