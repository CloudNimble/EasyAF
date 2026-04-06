using CloudNimble.SimpleMessageBus.Publish;
#if EFCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace CloudNimble.EasyAF.Business
{

    /// <summary>
    /// Represents the base class for all EasyAF business logic managers. Provides access to a database context
    /// and message publishing capabilities for implementing business operations and workflows.
    /// </summary> 
    /// <typeparam name="TContext">The type of the database context (DbContext) used for data operations.</typeparam>
    /// <remarks>
    /// This base class is designed to encapsulate business logic that requires database access and messaging capabilities.
    /// It's particularly useful for implementing complex business processes such as user registration, order processing,
    /// or any workflow that needs to coordinate database operations with message publishing for event-driven architectures.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class UserRegistrationManager : ManagerBase&lt;MyDbContext&gt;
    /// {
    ///     public UserRegistrationManager(MyDbContext context, IMessagePublisher publisher) 
    ///         : base(context, publisher) { }
    ///         
    ///     public async Task&lt;User&gt; RegisterUserAsync(string email, string password)
    ///     {
    ///         var user = new User { Email = email, Password = HashPassword(password) };
    ///         DataContext.Users.Add(user);
    ///         await DataContext.SaveChangesAsync();
    ///         
    ///         await MessagePublisher.PublishAsync(new UserRegisteredEvent { UserId = user.Id });
    ///         return user;
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ManagerBase<TContext>
    {

        #region Public Properties

        /// <summary>
        /// Gets the database context instance used for data operations.
        /// This context is injected through the constructor and provides access to the database.
        /// </summary>
        public TContext DataContext { get; private set; }

        /// <summary>
        /// Gets the message publisher instance used for publishing events and messages to the message bus.
        /// This publisher is injected through the constructor and enables event-driven architecture patterns.
        /// </summary>
        public IMessagePublisher MessagePublisher { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerBase{TContext}"/> class.
        /// </summary>
        /// <param name="dataContext">The database context instance for data operations. Should be injected by the DI container.</param>
        /// <param name="messagePublisher">The message publisher instance for publishing events. Should be injected by the DI container.</param>
        public ManagerBase(TContext dataContext, IMessagePublisher messagePublisher)
        {
            DataContext = dataContext;
            MessagePublisher = messagePublisher;
        }

        #endregion

    }

}
