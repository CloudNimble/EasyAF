using System;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// Specifies the type of interval duration.
    /// </summary>
    public enum IntervalType
    {

        /// <summary>
        /// Represents an interval measured in minutes.
        /// </summary>
        Minutes = 0,

        /// <summary>
        /// Represents an interval measured in hours.
        /// </summary>
        Hours = 1,

        /// <summary>
        /// Represents an interval measured in days.
        /// </summary>
        Days = 2,

        /// <summary>
        /// Represents an interval measured in weeks.
        /// </summary>
        Weeks = 3,

        /// <summary>
        /// Represents an interval measured in months.
        /// </summary>
        Months = 4,

        /// <summary>
        /// Represents an interval measured in quarters (3-month periods).
        /// </summary>
        Quarters = 5,

        /// <summary>
        /// Represents an interval measured in years.
        /// </summary>
        Years = 6

    }

}
