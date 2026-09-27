using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace CloudNimble.EasyAF.Http
{

    /// <summary>
    /// <see cref="IHttpContextAccessor"/> that keeps an outer request and an inner in-process
    /// request as two contexts. <see cref="HttpContext"/> always returns <see cref="Active"/>.
    /// <see cref="Start"/> / <see cref="End"/> change which context is active without using
    /// Microsoft's <c>HttpContextAccessor</c> setter (that setter nulls a shared holder and ends
    /// the outer request).
    /// </summary>
    /// <remarks>
    /// Register this as <see cref="IHttpContextAccessor"/> after <c>AddHttpContextAccessor</c> so
    /// last-wins DI gives the rest of the pipeline this instance.
    /// <para>
    /// Host <c>Initialize</c> assigns <see cref="HttpContext"/> (sets <see cref="Outer"/> and
    /// <see cref="Active"/>). Inner execute calls <see cref="Start"/>, then <see cref="End"/> in
    /// <c>finally</c>. Host <c>Dispose</c> assigns <see langword="null"/>, which poisons the
    /// holder so every execution context sees request-end.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// accessor.HttpContext = incomingRequest;
    /// InnerHttpContext.CopyFromOuter(innerRequest, incomingRequest);
    /// accessor.Start(innerRequest);
    /// try
    /// {
    ///     await pipeline(innerRequest);
    /// }
    /// finally
    /// {
    ///     accessor.End();
    /// }
    /// </code>
    /// </example>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "IHttpContextAccessor is an instance contract; state lives in AsyncLocal like Microsoft's accessor.")]
    public sealed class DualHttpContextAccessor : IHttpContextAccessor
    {

        #region Fields

        /// <summary>
        /// Per-execution-context holder. Not Microsoft's static slot.
        /// </summary>
        private static readonly AsyncLocal<Holder> Current = new AsyncLocal<Holder>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the context <see cref="HttpContext"/> returns. <see cref="Start"/> and
        /// <see cref="End"/> change this without using the setter.
        /// </summary>
        public HttpContext Active
        {
            get
            {
                return Current.Value?.Active;
            }
        }

        /// <inheritdoc />
        public HttpContext HttpContext
        {
            get
            {
                return Active;
            }
            set
            {
                var holder = Current.Value;
                if (holder is not null)
                {
                    holder.Active = null;
                    holder.Inner = null;
                    holder.Outer = null;
                }

                if (value is null)
                {
                    return;
                }

                Current.Value = new Holder
                {
                    Active = value,
                    Outer = value
                };
            }
        }

        /// <summary>
        /// Gets the inner in-process context, or <see langword="null"/> if <see cref="Start"/> has not run
        /// or <see cref="End"/> / request-end has already cleared it.
        /// </summary>
        public HttpContext Inner
        {
            get
            {
                return Current.Value?.Inner;
            }
        }

        /// <summary>
        /// Gets the incoming (outer) context, or <see langword="null"/> if the host has not assigned
        /// <see cref="HttpContext"/> yet.
        /// </summary>
        public HttpContext Outer
        {
            get
            {
                return Current.Value?.Outer;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Makes <see cref="HttpContext"/> return <see cref="Outer"/> again and clears
        /// <see cref="Inner"/>. Does not write to <see cref="Outer"/>'s response.
        /// </summary>
        public void End()
        {
            var holder = Current.Value;
            if (holder is null)
            {
                return;
            }

            holder.Active = holder.Outer;
            holder.Inner = null;
        }

        /// <summary>
        /// Makes <see cref="HttpContext"/> return <paramref name="inner"/> without dropping
        /// <see cref="Outer"/>. Do not use the <see cref="HttpContext"/> setter for this.
        /// </summary>
        /// <param name="inner">The inner in-process request.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is <see langword="null"/>.</exception>
        public void Start(HttpContext inner)
        {
            ArgumentNullException.ThrowIfNull(inner);

            var holder = Current.Value;
            if (holder is null)
            {
                Current.Value = new Holder
                {
                    Active = inner,
                    Inner = inner
                };

                return;
            }

            holder.Active = inner;
            holder.Inner = inner;
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Mutable box so request-end can clear every execution context that still holds this instance.
        /// </summary>
        internal sealed class Holder
        {

            #region Properties

            /// <summary>
            /// Gets or sets the context everyone else sees through <see cref="DualHttpContextAccessor.HttpContext"/>.
            /// </summary>
            internal HttpContext Active { get; set; }

            /// <summary>
            /// Gets or sets the inner in-process context.
            /// </summary>
            internal HttpContext Inner { get; set; }

            /// <summary>
            /// Gets or sets the incoming (outer) context.
            /// </summary>
            internal HttpContext Outer { get; set; }

            #endregion

        }

        #endregion

    }

}
