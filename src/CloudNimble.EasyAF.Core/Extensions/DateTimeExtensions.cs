namespace System
{

    /// <summary>
    /// Extensions on <see cref="DateTime"/> and <see cref="DateTimeOffset"/>.
    /// </summary>
    public static class EasyAF_DateTimeExtensions
    {

        /// <summary>
        /// Calculates the quarter for the given <see cref="DateTime"/>, assuming a calendar-based fiscal year.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to use in the calculation.</param>
        /// <returns></returns>
        /// <remarks>
        /// From https://stackoverflow.com/questions/8698303/how-do-i-discover-the-quarter-of-a-given-date
        /// </remarks>
        public static int GetQuarter(this DateTime date)
        {
            return (date.Month + 2) / 3;
        }

        /// <summary>
        /// Calculates the quarter for the given <see cref="DateTime"/>, assuming a the provided fiscal year begin date.
        /// </summary>
        /// <param name="date">The <see cref="DateTime"/> to use in the calculation.</param>
        /// <param name="fiscalYearStart">The <see cref="DateTime"/> representing the start day of the fiscal year to use in calculation.</param>
        /// <returns></returns>
        /// <remarks>
        /// From https://stackoverflow.com/questions/8698303/how-do-i-discover-the-quarter-of-a-given-date
        /// </remarks>
        public static int GetQuarter(this DateTime date, DateTime fiscalYearStart)
        {
            var adjustor = date.Month < fiscalYearStart.Month ? 12 : 0;
            var numerator = date.Month + adjustor - fiscalYearStart.Month;
            var quotient = numerator / 3;
            var result = 1 + quotient;
            return result;
        }

        /// <summary>
        /// Calculates the quarter for the given <see cref="DateTimeOffset"/>, assuming a calendar-based fiscal year.
        /// </summary>
        /// <param name="date">The <see cref="DateTimeOffset"/> to use in the calculation.</param>
        /// <returns></returns>
        /// <remarks>
        /// From https://stackoverflow.com/questions/8698303/how-do-i-discover-the-quarter-of-a-given-date
        /// </remarks>
        public static int GetQuarter(this DateTimeOffset date)
        {
            return date.DateTime.GetQuarter();
        }

        /// <summary>
        /// Calculates the quarter for the given <see cref="DateTimeOffset"/>, assuming a the provided fiscal year begin date.
        /// </summary>
        /// <param name="date">The <see cref="DateTimeOffset"/> to use in the calculation.</param>
        /// <param name="fiscalYearStart">The <see cref="DateTime"/> representing the start day of the fiscal year to use in calculation.</param>
        /// <returns></returns>
        /// <remarks>
        /// From https://stackoverflow.com/questions/8698303/how-do-i-discover-the-quarter-of-a-given-date
        /// </remarks>
        public static int GetQuarter(this DateTimeOffset date, DateTimeOffset fiscalYearStart)
        {
            return date.DateTime.GetQuarter(fiscalYearStart.DateTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/24245523/getting-the-first-and-last-day-of-a-month-using-a-given-datetime-object
        /// </remarks>
        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/24245523/getting-the-first-and-last-day-of-a-month-using-a-given-datetime-object
        /// </remarks>
        public static DateTimeOffset FirstDayOfMonth(this DateTimeOffset value)
        {
            return new DateTimeOffset(value.Year, value.Month, 1, 0, 0, 0, value.Offset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/24245523/getting-the-first-and-last-day-of-a-month-using-a-given-datetime-object
        /// </remarks>
        public static int DaysInMonth(this DateTime value)
        {
            return DateTime.DaysInMonth(value.Year, value.Month);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/24245523/getting-the-first-and-last-day-of-a-month-using-a-given-datetime-object
        /// </remarks>
        public static int DaysInMonth(this DateTimeOffset value)
        {
            return DateTime.DaysInMonth(value.Year, value.Month);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/24245523/getting-the-first-and-last-day-of-a-month-using-a-given-datetime-object
        /// </remarks>

        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.DaysInMonth());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/24245523/getting-the-first-and-last-day-of-a-month-using-a-given-datetime-object
        /// </remarks>
        public static DateTimeOffset LastDayOfMonth(this DateTimeOffset value)
        {
            return new DateTimeOffset(value.Year, value.Month, value.DaysInMonth(), 0, 0, 0, value.Offset);
        }

    }

}
