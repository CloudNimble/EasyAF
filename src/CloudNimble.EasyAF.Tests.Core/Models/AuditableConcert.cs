using CloudNimble.EasyAF.Core;
using System;

namespace CloudNimble.EasyAF.Tests.Core.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class AuditableConcert : Concert, ICreatedAuditable
    {

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset DateCreated { get; set; }

    }

}
