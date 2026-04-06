using CloudNimble.EasyAF.Core;
using System;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.Tests.Core.Models
{

    public class Concert : DbObservableObject
    {

        #region Private Members

        Guid id;
        Person organizer;
        List<Person> attendees;

        #endregion

        #region Properties

        public Guid Id
        {
            get => id;
            set => Set(() => Id, ref id, value);
        }

        public Person Organizer
        {
            get => organizer;
            set => Set(() => Organizer, ref organizer, value);
        }

        public List<Person> Attendees
        {
            get => attendees;
            set => Set(() => Attendees, ref attendees, value);
        }

        #endregion

    }

}
