using CloudNimble.EasyAF.Core;

namespace CloudNimble.EasyAF.Tests.Core.Models
{

    public class Person : DbObservableObject
    {

        #region Private Members

        string firstName;
        string lastName;

        #endregion

        #region Properties

        public string FirstName
        {
            get => firstName;
            set => Set(() => FirstName, ref firstName, value);
        }

        public string LastName
        {
            get => lastName;
            set => Set(() => LastName, ref lastName, value);

        }

        #endregion

    }

}
