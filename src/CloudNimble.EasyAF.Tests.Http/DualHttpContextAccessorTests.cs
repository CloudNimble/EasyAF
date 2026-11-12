using System;
using System.Threading.Tasks;
using CloudNimble.EasyAF.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudNimble.EasyAF.Tests.Http
{

    /// <summary>
    /// <see cref="DualHttpContextAccessor"/> swaps Active/Outer/Inner without Microsoft's setter.
    /// </summary>
    [TestClass]
    public class DualHttpContextAccessorTests
    {

        #region Public Methods

        /// <summary>
        /// After <c>End</c>, <c>HttpContext</c> and <see cref="DualHttpContextAccessor.Active"/> are the outer
        /// request and <see cref="DualHttpContextAccessor.Inner"/> is <see langword="null"/>.
        /// </summary>
        [TestMethod]
        public void End_RestoresActiveToOuter_ClearsInner()
        {
            var accessor = new DualHttpContextAccessor();
            var outer = new DefaultHttpContext();
            var inner = new DefaultHttpContext();
            accessor.HttpContext = outer;
            accessor.Start(inner);

            accessor.End();

            accessor.HttpContext.Should().BeSameAs(outer);
            accessor.Active.Should().BeSameAs(outer);
            accessor.Outer.Should().BeSameAs(outer);
            accessor.Inner.Should().BeNull();
        }

        /// <summary>
        /// Host <c>Dispose</c> (<c>HttpContext = null</c>) clears Outer, Active, and Inner on the shared holder.
        /// </summary>
        [TestMethod]
        public void HttpContext_SetNull_ClearsOuterActiveAndInner()
        {
            var accessor = new DualHttpContextAccessor();
            var outer = new DefaultHttpContext();
            var inner = new DefaultHttpContext();
            accessor.HttpContext = outer;
            accessor.Start(inner);

            accessor.HttpContext = null;

            accessor.HttpContext.Should().BeNull();
            accessor.Active.Should().BeNull();
            accessor.Outer.Should().BeNull();
            accessor.Inner.Should().BeNull();
        }

        /// <summary>
        /// Host <c>Initialize</c> (<c>HttpContext = request</c>) loads Outer and Active.
        /// </summary>
        [TestMethod]
        public void HttpContext_Set_LoadsOuterAndActive()
        {
            var accessor = new DualHttpContextAccessor();
            var outer = new DefaultHttpContext();

            accessor.HttpContext = outer;

            accessor.HttpContext.Should().BeSameAs(outer);
            accessor.Active.Should().BeSameAs(outer);
            accessor.Outer.Should().BeSameAs(outer);
            accessor.Inner.Should().BeNull();
        }

        /// <summary>
        /// Inner/outer survive <c>await</c> on the same execution context.
        /// </summary>
        [TestMethod]
        public async Task HttpContext_FlowsAcrossAwait()
        {
            var accessor = new DualHttpContextAccessor();
            var outer = new DefaultHttpContext();
            var inner = new DefaultHttpContext();
            accessor.HttpContext = outer;
            accessor.Start(inner);

            await Task.Yield();

            accessor.HttpContext.Should().BeSameAs(inner);
            accessor.Active.Should().BeSameAs(inner);
            accessor.Outer.Should().BeSameAs(outer);
            accessor.Inner.Should().BeSameAs(inner);

            accessor.End();
            await Task.Yield();

            accessor.HttpContext.Should().BeSameAs(outer);
            accessor.Active.Should().BeSameAs(outer);
            accessor.Inner.Should().BeNull();
        }

        /// <summary>
        /// <c>Start</c> rejects a null inner context.
        /// </summary>
        [TestMethod]
        public void Start_NullInner_Throws()
        {
            var accessor = new DualHttpContextAccessor();

            var act = () => accessor.Start(null);

            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// <c>Start</c> makes <c>HttpContext</c> the inner request without dropping the outer one.
        /// </summary>
        [TestMethod]
        public void Start_SwapsActiveToInner_KeepsOuter()
        {
            var accessor = new DualHttpContextAccessor();
            var outer = new DefaultHttpContext();
            var inner = new DefaultHttpContext();
            accessor.HttpContext = outer;

            accessor.Start(inner);

            accessor.HttpContext.Should().BeSameAs(inner);
            accessor.Active.Should().BeSameAs(inner);
            accessor.Outer.Should().BeSameAs(outer);
            accessor.Inner.Should().BeSameAs(inner);
        }

        #endregion

    }

}
