using CloudNimble.EasyAF.Core;

namespace CloudNimble.EasyAF.Tests.Core.Models
{

    /// <summary>
    /// A test entity with an <see cref="int"/> ID, for code paths that must not assume <see cref="System.Guid"/> IDs.
    /// </summary>
    public class Badge : DbObservableObject, IIdentifiable<int>
    {

        #region Private Members

        int id;
        string displayName;

        #endregion

        #region Properties

        public int Id
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
