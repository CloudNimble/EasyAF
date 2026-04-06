using CloudNimble.SimpleMessageBus.Publish;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

#if EFCORE
using Microsoft.EntityFrameworkCore;
using Microsoft.Restier.EntityFrameworkCore;
#else
using System.Data.Entity;
using Microsoft.Restier.EntityFramework;
#endif

namespace CloudNimble.EasyAF.Restier
{

    /// <summary>
    /// Provides a base implementation of an Entity Framework API for EasyAF, 
    /// integrating SimpleMessageBus event publishing and logging capabilities.
    /// <para>
    /// This class extends <see cref="EntityFrameworkApi{TContext}"/> and is intended to be used as a base class 
    /// for APIs that require access to the current HTTP context, logging, and SimpleMessageBus publishing.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/> used by the API.</typeparam>
    /// <example>
    /// <code>
    /// public class MyApi : EasyAFEntityFrameworkApi&lt;MyDbContext&gt;
    /// {
    ///     public MyApi(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, IMessagePublisher messagePublisher, ILogger&lt;EasyAFEntityFrameworkApi&lt;MyDbContext&gt;&gt; logger)
    ///         : base(serviceProvider, httpContextAccessor, messagePublisher, logger)
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class EasyAFEntityFrameworkApi<TContext> : EntityFrameworkApi<TContext>
        where TContext : DbContext
    {

        #region Public Properties

        /// <summary>
        /// Gets or sets the accessor for the current HTTP context.
        /// Used to access HTTP-specific information about the current request.
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ILogger{T}" /> instance used for writing log traces.
        /// </summary>
        public ILogger<EasyAFEntityFrameworkApi<TContext>> Logger { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IMessagePublisher"/> used for publishing messages to SimpleMessageBus.
        /// </summary>
        public IMessagePublisher MessagePublisher { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyAFEntityFrameworkApi{TContext}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> for the current HTTP context.</param>
        /// <param name="messagePublisher">The <see cref="IMessagePublisher"/> used for publishing messages to SimpleMessageBus.</param>
        /// <param name="logger">The <see cref="ILogger{T}" /> instance for writing log traces.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="httpContextAccessor"/> or <paramref name="messagePublisher"/> is <c>null</c>.
        /// </exception>
        public EasyAFEntityFrameworkApi(
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            IMessagePublisher messagePublisher,
            ILogger<EasyAFEntityFrameworkApi<TContext>> logger) : base(serviceProvider)
        {
            HttpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            MessagePublisher = messagePublisher
                ?? throw new ArgumentNullException(nameof(messagePublisher));
            Logger = logger 
                ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

    }

}
