using CloudNimble.EasyAF.Core;
using System;

namespace CloudNimble.EasyAF.Tests.Core.Models
{

    /// <summary>
    /// An auditable entity that contains another auditable entity, to verify audit fields are ignored throughout the graph.
    /// </summary>
    public class AuditableTour : DbObservableObject, ICreatedAuditable
    {

        #region Private Members

        DateTimeOffset dateCreated;
        AuditableConcert headliner;
        string name;

        #endregion

        #region Properties

        public DateTimeOffset DateCreated
        {
            get => dateCreated;
            set => Set(nameof(DateCreated), ref dateCreated, value);
        }

        public AuditableConcert Headliner
        {
            get => headliner;
            set => Set(nameof(Headliner), ref headliner, value);
        }

        public string Name
        {
            get => name;
            set => Set(nameof(Name), ref name, value);
        }

        #endregion

    }

}
