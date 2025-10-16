using Humanizer;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// Represents a sum of money to be exchanged during a given interval.
    /// </summary>
    /// <remarks>
    /// This has been broken up to allow for conversions (for example, converting $/month into $/day) to be self-contained. This should reduce duplication.
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class MoneyInterval<T> : Interval<T> where T : IComparable<T>, IConvertible
    {

        #region Properties

        /// <summary>
        /// The amount of money represented by the given <see cref="IntervalType"/>
        /// </summary>
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:$###,###,##0;($###,###,##0);$0}", NullDisplayText = "$0")]
        public decimal Money { get; set; }

        /// <summary>
        /// Returns a string suitable for display in the debugger. Ensures such strings are compiled by the runtime and not interpreted by the currently-executing language.
        /// </summary>
        /// <remarks>http://blogs.msdn.com/b/jaredpar/archive/2011/03/18/debuggerdisplay-attribute-best-practices.aspx</remarks>
        private string DebuggerDisplay => $"Interval: {Value} {Type}, Money: ${Money}";

        #endregion

        #region Constructors

        /// <summary>
        /// </summary>
        public MoneyInterval()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoneyInterval{T}"/> class with the specified interval value and type.
        /// </summary>
        /// <param name="value">The duration of the interval.</param>
        /// <param name="type">The base unit that describes what the quantity of this interval references.</param>
        public MoneyInterval(T value, IntervalType type) : base(value, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoneyInterval{T}"/> class with the specified money amount, interval value, and type.
        /// </summary>
        /// <param name="money">The amount of money represented by the given interval.</param>
        /// <param name="value">The duration of the interval.</param>
        /// <param name="type">The base unit that describes what the quantity of this interval references.</param>
        public MoneyInterval(decimal money, T value, IntervalType type) : this(value, type)
        {
            Money = money;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the monetary amount per minute based on this money interval.
        /// </summary>
        /// <returns>The amount of money per minute as a decimal value.</returns>
        public override decimal PerMinute()
        {
            return base.PerMinute() * Money;
        }

        /// <summary>
        /// Calculates the total monetary amount per minute based on this money interval and a quantity multiplier.
        /// </summary>
        /// <param name="quantity">The quantity multiplier (e.g., hours worked, units sold).</param>
        /// <returns>The total amount of money per minute as a decimal value.</returns>
        /// <example>
        /// <code>
        /// // $25 per hour wage, calculate earnings for 8 hours of work per minute
        /// var wage = new MoneyInterval&lt;double&gt;(25m, 1, IntervalType.Hours);
        /// decimal totalPerMinute = wage.PerMinute(8); // $3.33 per minute (25 * 8 / 60)
        /// </code>
        /// </example>
        public override decimal PerMinute(decimal quantity)
        {
            return PerMinute() * quantity;
        }

        /// <summary>
        /// Calculates the monetary amount per hour based on this money interval.
        /// </summary>
        /// <returns>The amount of money per hour as a decimal value.</returns>
        public override decimal PerHour()
        {
            return base.PerHour() * Money;
        }

        /// <summary>
        /// Calculates the total monetary amount per hour based on this money interval and a quantity multiplier.
        /// </summary>
        /// <param name="quantity">The quantity multiplier (e.g., hours worked, units sold).</param>
        /// <returns>The total amount of money per hour as a decimal value.</returns>
        /// <example>
        /// <code>
        /// // $25 per hour wage, calculate earnings for 8 hours of work per hour
        /// var wage = new MoneyInterval&lt;double&gt;(25m, 1, IntervalType.Hours);
        /// decimal totalPerHour = wage.PerHour(8); // $200 per hour (25 * 8)
        /// </code>
        /// </example>
        public override decimal PerHour(decimal quantity)
        {
            return PerHour() * quantity;
        }

        /// <summary>
        /// Calculates the monetary amount per day based on this money interval.
        /// </summary>
        /// <returns>The amount of money per day as a decimal value.</returns>
        public override decimal PerDay()
        {
            return base.PerDay() * Money;
        }

        /// <summary>
        /// Calculates the total monetary amount per day based on this money interval and a quantity multiplier.
        /// </summary>
        /// <param name="quantity">The quantity multiplier (e.g., hours worked, units sold).</param>
        /// <returns>The total amount of money per day as a decimal value.</returns>
        /// <example>
        /// <code>
        /// // $25 per hour wage, calculate earnings for 8 hours of work per day
        /// var wage = new MoneyInterval&lt;double&gt;(25m, 1, IntervalType.Hours);
        /// decimal totalPerDay = wage.PerDay(8); // $4800 per day (25 * 24 * 8)
        /// </code>
        /// </example>
        public override decimal PerDay(decimal quantity)
        {
            return PerDay() * quantity;
        }

        /// <summary>
        /// Calculates the monetary amount per week based on this money interval.
        /// </summary>
        /// <returns>The amount of money per week as a decimal value.</returns>
        public override decimal PerWeek()
        {
            return base.PerWeek() * Money;
        }

        /// <summary>
        /// Calculates the total monetary amount per week based on this money interval and a quantity multiplier.
        /// </summary>
        /// <param name="quantity">The quantity multiplier (e.g., hours worked, units sold).</param>
        /// <returns>The total amount of money per week as a decimal value.</returns>
        /// <example>
        /// <code>
        /// // $150 every 2.5 hours, calculate earnings for 40 hours of work per week
        /// var freelance = new MoneyInterval&lt;double&gt;(150m, 2.5, IntervalType.Hours);
        /// decimal totalPerWeek = freelance.PerWeek(40); // $40,320 per week
        /// </code>
        /// </example>
        public override decimal PerWeek(decimal quantity)
        {
            return PerWeek() * quantity;
        }

        /// <summary>
        /// Calculates the monetary amount per month based on this money interval.
        /// </summary>
        /// <returns>The amount of money per month as a decimal value.</returns>
        public override decimal PerMonth()
        {
            return base.PerMonth() * Money;
        }

        /// <summary>
        /// Calculates the total monetary amount per month based on this money interval and a quantity multiplier.
        /// </summary>
        /// <param name="quantity">The quantity multiplier (e.g., hours worked, units sold).</param>
        /// <returns>The total amount of money per month as a decimal value.</returns>
        /// <example>
        /// <code>
        /// // $50 per day, calculate earnings for 20 working days per month
        /// var dailyRate = new MoneyInterval&lt;double&gt;(50m, 1, IntervalType.Days);
        /// decimal totalPerMonth = dailyRate.PerMonth(20); // $30,000 per month (50 * 30 * 20)
        /// </code>
        /// </example>
        public override decimal PerMonth(decimal quantity)
        {
            return PerMonth() * quantity;
        }

        /// <summary>
        /// Calculates the monetary amount per year based on this money interval.
        /// </summary>
        /// <returns>The amount of money per year as a decimal value.</returns>
        public override decimal PerYear()
        {
            return base.PerYear() * Money;
        }

        /// <summary>
        /// Calculates the total monetary amount per year based on this money interval and a quantity multiplier.
        /// </summary>
        /// <param name="quantity">The quantity multiplier (e.g., hours worked, units sold).</param>
        /// <returns>The total amount of money per year as a decimal value.</returns>
        /// <example>
        /// <code>
        /// // $75,000 annual salary, calculate total compensation with 1.2x multiplier
        /// var salary = new MoneyInterval&lt;double&gt;(75000m, 1, IntervalType.Years);
        /// decimal totalPerYear = salary.PerYear(1.2m); // $90,000 per year (75000 * 1.2)
        /// </code>
        /// </example>
        public override decimal PerYear(decimal quantity)
        {
            return PerYear() * quantity;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Money:C} / {Value} {Type.Humanize().ToQuantity((int)Math.Round(Convert.ToDecimal(Value), MidpointRounding.AwayFromZero), ShowQuantityAs.None)}";
        }

        /// <summary>
        /// Returns a string representation of the money interval with the specified number of decimal places for the currency value.
        /// </summary>
        /// <param name="decimals">The number of decimal places to display for the currency value.</param>
        /// <returns>A formatted string showing the money amount per interval period.</returns>
        public string ToString(int decimals)
        {
            return $"{Money.ToString($"C{decimals}")} / {Value} {Type.Humanize().ToQuantity((int)Math.Round(Convert.ToDecimal(Value), MidpointRounding.AwayFromZero), ShowQuantityAs.None)}";
        }

        #endregion

    }

}
