using CloudNimble.EasyAF.Core;
using System;

namespace CloudNimble.EasyAF.Tests.Core.Models
{

    public class Department : DbObservableObject, IIdentifiable<Guid>
    {

        #region Private Members

        Guid id;
        string displayName;

        #endregion

        #region Properties

        public Guid Id
        {
            get => id;
            set => Set(() => Id, ref id, value);
        }

        public string DisplayName
        {
            get => displayName;
            set => Set(() => DisplayName, ref displayName, value);
        }

        #endregion

    }

}
