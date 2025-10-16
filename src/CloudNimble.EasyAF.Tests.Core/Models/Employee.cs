using CloudNimble.EasyAF.Core;
using System;

namespace CloudNimble.EasyAF.Tests.Core.Models
{

    public class Employee : DbObservableObject, IIdentifiable<Guid>
    {

        #region Private Members

        Guid id;
        Guid departmentId;
        Person person;
        string title;

        #endregion

        #region Properties

        public Guid Id
        {
            get => id;
            set => Set(() => Id, ref id, value);
        }

        public Guid DepartmentId
        {
            get => departmentId;
            set => Set(() => DepartmentId, ref departmentId, value);
        }

        public Person Person
        {
            get => person;
            set => Set(() => Person, ref person, value);
        }

        public string Title
        {
            get => title;
            set => Set(() => Title, ref title, value);
        }

        #endregion

    }

}
