using Humanizer;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// Describes an interval of time to be used in time-based calculations.
    /// Provides methods to calculate rates and frequencies based on the interval value and type.
    /// </summary>
    /// <typeparam name="T">The data type for the interval value. Must implement <see cref="IComparable{T}"/> and <see cref="IConvertible"/>.</typeparam>
    /// <example>
    /// <code>
    /// // Create an interval representing something that happens every 3 hours
    /// var interval = new Interval&lt;int&gt;(3, IntervalType.Hours);
    /// 
    /// // Calculate how many times per day this would occur
    /// decimal timesPerDay = interval.PerDay(); // Returns 8.0
    /// 
    /// // Calculate how many minutes between occurrences  
    /// decimal minutesBetween = interval.PerMinute(); // Returns 0.0556 (1/18)
    /// </code>
    /// </example>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Interval<T> where T : IComparable<T>, IConvertible
    {

        #region Properties

        /// <summary>
        /// The base unit that describes what the quantity of this Interval references.
        /// </summary>
        public IntervalType Type { get; set; }

        /// <summary>
        /// The duration of the Interval.
        /// </summary>
        [DataType(DataType.Duration)]
        public T Value { get; set; }

        /// <summary>
        /// Returns a string suitable for display in the debugger. Ensures such strings are compiled by the runtime and not interpreted by the currently-executing language.
        /// </summary>
        /// <remarks>http://blogs.msdn.com/b/jaredpar/archive/2011/03/18/debuggerdisplay-attribute-best-practices.aspx</remarks>
        private string DebuggerDisplay => $"Interval: {Value} {Type}";

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="Interval{T}"/> class.
        /// </summary>
        public Interval()
        {
            Type = IntervalType.Months;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Interval{T}"/> class.
        /// </summary>
        /// <param name="value">The duration of the interval.</param>
        /// <param name="type">The base unit that describes what the quantity of this Interval references.</param>
        public Interval(T value, IntervalType type)
        {
            Type = type;
            Value = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance, calculates how many occurrences will happen per minute.
        /// </summary>
        /// <returns>The number of occurrences per minute as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        /// <remarks>If you need this as a whole number, wrap the result in <see cref="Math.Floor(decimal)"/>.</remarks>
        public virtual decimal PerMinute()
        {
            return Type switch
            {
                IntervalType.Minutes => Convert.ToDecimal(Value),
                IntervalType.Hours => Convert.ToDecimal(Value) / 60,
                IntervalType.Days => Convert.ToDecimal(Value) / 1440,
                IntervalType.Weeks => Convert.ToDecimal(Value) / 10080,
                IntervalType.Months => Convert.ToDecimal(Value) / 43800,
                IntervalType.Years => Convert.ToDecimal(Value) / 525600,
                _ => Convert.ToDecimal(Value) / 1,
            };
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance and a quantity, calculates the total output per minute.
        /// </summary>
        /// <param name="quantity">The quantity to multiply by the interval frequency.</param>
        /// <returns>The total output per minute as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        /// <example>
        /// <code>
        /// // Widget production: 1 widget every 90 minutes, total from 100 units of material per minute
        /// var production = new Interval&lt;int&gt;(90, IntervalType.Minutes);
        /// decimal totalPerMinute = production.PerMinute(100); // 1.11 widgets per minute (1/90 * 100)
        /// </code>
        /// </example>
        public virtual decimal PerMinute(decimal quantity)
        {
            return PerMinute() * quantity;
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance, calculates how many occurrences will happen per hour.
        /// </summary>
        /// <returns>The number of occurrences per hour as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        public virtual decimal PerHour()
        {
            return Type switch
            {
                IntervalType.Minutes => 60 / Convert.ToDecimal(Value),
                IntervalType.Hours => Convert.ToDecimal(Value),
                IntervalType.Days => Convert.ToDecimal(Value) / 24,
                IntervalType.Weeks => Convert.ToDecimal(Value) / 168,
                IntervalType.Months => Convert.ToDecimal(Value) / 730,
                IntervalType.Years => Convert.ToDecimal(Value) / 8760,
                _ => Convert.ToDecimal(Value) / 1,
            };
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance and a quantity, calculates the total output per hour.
        /// </summary>
        /// <param name="quantity">The quantity to multiply by the interval frequency.</param>
        /// <returns>The total output per hour as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        /// <example>
        /// <code>
        /// // Widget production: 1 widget every 1.5 hours, total from 100 units of material per hour
        /// var production = new Interval&lt;double&gt;(1.5, IntervalType.Hours);
        /// decimal totalPerHour = production.PerHour(100); // 66.67 widgets per hour (1/1.5 * 100)
        /// </code>
        /// </example>
        public virtual decimal PerHour(decimal quantity)
        {
            return PerHour() * quantity;
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance, calculates how many occurrences will happen per day.
        /// </summary>
        /// <returns>The number of occurrences per day as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        public virtual decimal PerDay()
        {
            return Type switch
            {
                IntervalType.Minutes => 1440 / Convert.ToDecimal(Value),
                IntervalType.Hours => 24 / Convert.ToDecimal(Value),
                IntervalType.Days => Convert.ToDecimal(Value),
                IntervalType.Weeks => Convert.ToDecimal(Value) / 7,
                IntervalType.Months => Convert.ToDecimal(Value) / 30.4166667M,
                IntervalType.Years => Convert.ToDecimal(Value) / 365,
                _ => Convert.ToDecimal(Value) / 1,
            };
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance and a quantity, calculates the total output per day.
        /// </summary>
        /// <param name="quantity">The quantity to multiply by the interval frequency.</param>
        /// <returns>The total output per day as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        /// <example>
        /// <code>
        /// // Widget production: 1 widget every 1.5 hours, total from 100 units of material per day
        /// var production = new Interval&lt;double&gt;(1.5, IntervalType.Hours);
        /// decimal totalPerDay = production.PerDay(100); // 1600 widgets per day (16 * 100)
        /// </code>
        /// </example>
        public virtual decimal PerDay(decimal quantity)
        {
            return PerDay() * quantity;
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance, calculates how many occurrences will happen per week.
        /// </summary>
        /// <returns>The number of occurrences per week as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        public virtual decimal PerWeek()
        {
            return Type switch
            {
                IntervalType.Minutes => 10080 / Convert.ToDecimal(Value),
                IntervalType.Hours => 168 / Convert.ToDecimal(Value),
                IntervalType.Days => 7 / Convert.ToDecimal(Value),
                IntervalType.Weeks => Convert.ToDecimal(Value),
                IntervalType.Months => Convert.ToDecimal(Value) / 4.3452381M,
                IntervalType.Years => Convert.ToDecimal(Value) / 52.1428571M,
                _ => Convert.ToDecimal(Value) / 1,
            };
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance and a quantity, calculates the total output per week.
        /// </summary>
        /// <param name="quantity">The quantity to multiply by the interval frequency.</param>
        /// <returns>The total output per week as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        /// <example>
        /// <code>
        /// // Widget production: 1 widget every 2 days, total from 50 units of material per week
        /// var production = new Interval&lt;int&gt;(2, IntervalType.Days);
        /// decimal totalPerWeek = production.PerWeek(50); // 175 widgets per week (3.5 * 50)
        /// </code>
        /// </example>
        public virtual decimal PerWeek(decimal quantity)
        {
            return PerWeek() * quantity;
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance, calculates how many occurrences will happen per month.
        /// </summary>
        /// <returns>The number of occurrences per month as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        public virtual decimal PerMonth()
        {
            return Type switch
            {
                IntervalType.Minutes => 43800 / Convert.ToDecimal(Value),
                IntervalType.Hours => 730 / Convert.ToDecimal(Value),
                IntervalType.Days => 30 / Convert.ToDecimal(Value),
                IntervalType.Weeks => 4.3452381M / Convert.ToDecimal(Value),
                IntervalType.Months => Convert.ToDecimal(Value),
                IntervalType.Years => Convert.ToDecimal(Value) / 12,
                _ => Convert.ToDecimal(Value) / 1,
            };
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance and a quantity, calculates the total output per month.
        /// </summary>
        /// <param name="quantity">The quantity to multiply by the interval frequency.</param>
        /// <returns>The total output per month as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        /// <example>
        /// <code>
        /// // Widget production: 1 widget every 3 days, total from 200 units of material per month
        /// var production = new Interval&lt;int&gt;(3, IntervalType.Days);
        /// decimal totalPerMonth = production.PerMonth(200); // 2000 widgets per month (10 * 200)
        /// </code>
        /// </example>
        public virtual decimal PerMonth(decimal quantity)
        {
            return PerMonth() * quantity;
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance, calculates how many occurrences will happen per year.
        /// </summary>
        /// <returns>The number of occurrences per year as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        public virtual decimal PerYear()
        {
            return Type switch
            {
                IntervalType.Minutes => 525600 / Convert.ToDecimal(Value),
                IntervalType.Hours => 8760 / Convert.ToDecimal(Value),
                IntervalType.Days => 365 / Convert.ToDecimal(Value),
                IntervalType.Weeks => 52.1428571M / Convert.ToDecimal(Value),
                IntervalType.Months => 12 / Convert.ToDecimal(Value),
                IntervalType.Years => Convert.ToDecimal(Value),
                _ => Convert.ToDecimal(Value) / 1,
            };
        }

        /// <summary>
        /// Given this <see cref="Interval{T}"/> instance and a quantity, calculates the total output per year.
        /// </summary>
        /// <param name="quantity">The quantity to multiply by the interval frequency.</param>
        /// <returns>The total output per year as a decimal value.</returns>
        /// <exception cref="InvalidCastException">Thrown if <typeparamref name="T"/> is not convertible to a <see cref="decimal"/>.</exception>
        /// <example>
        /// <code>
        /// // Widget production: 1 widget every 1 week, total from 500 units of material per year
        /// var production = new Interval&lt;int&gt;(1, IntervalType.Weeks);
        /// decimal totalPerYear = production.PerYear(500); // 26071 widgets per year (52.14 * 500)
        /// </code>
        /// </example>
        public virtual decimal PerYear(decimal quantity)
        {
            return PerYear() * quantity;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Value} {Type.Humanize().ToQuantity((int)Math.Round(Convert.ToDecimal(Value), MidpointRounding.AwayFromZero), ShowQuantityAs.None)}";
        }

        #endregion

    }

}
