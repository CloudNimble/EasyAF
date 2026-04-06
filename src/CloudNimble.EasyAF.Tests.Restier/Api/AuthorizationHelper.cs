using EasyAFModel;
using Microsoft.Restier.Core.Authorization;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.Tests.Restier.Api
{

    /// <summary>
    /// 
    /// </summary>
    public static class AuthorizationHelper
    {

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public static void Configure()
        {
            static bool trueAction() => true;

            var entries = new List<AuthorizationEntry>
            {
                new(typeof(Product), trueAction, trueAction, trueAction),
            };
            AuthorizationFactory.RegisterEntries(entries);
        }

        #endregion

    }
}
