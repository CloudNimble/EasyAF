using Microsoft.Restier.AspNetCore.Model;
using Microsoft.Restier.EntityFrameworkCore;
using System;
using System.Security;

namespace CloudNimble.EasyAF.Tests.OData.Fakes
{

    /// <summary>
    /// A fake Restier API for unit testing.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    public class FakeApi : EntityFrameworkApi<FakeContext>
    {

        /// <summary>
        /// Constructor overload to pass <see cref="IServiceProvider"/> to the base class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FakeApi(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [UnboundOperation(OperationType = OperationType.Action)]
        public void SomeFaultyAction()
        {
            throw new SecurityException("Something went wrong!");
        }

        [UnboundOperation(OperationType = OperationType.Function)]
        public bool SomeValidFunction()
        {
            return true;
        }



    }

}
