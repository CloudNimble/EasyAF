using System;
using System.Diagnostics;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// Represents a ratio value that occurs at regular time intervals, enabling conversion between different time periods.
    /// This class combines a base time interval (from the <see cref="Interval{T}"/> class) with a ratio value to calculate 
    /// total ratio amounts across different time periods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Key Concepts:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Value:</strong> Always represents a time duration (e.g., 1.5 hours, 2 days)</description></item>
    /// <item><description><strong>Type:</strong> The time unit for the Value (Hours, Days, Months, etc.)</description></item>
    /// <item><description><strong>Ratio:</strong> The decimal ratio that occurs each interval period</description></item>
    /// </list>
    /// 
    /// <para>
    /// <strong>Method Types:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Per* methods (inherited):</strong> Calculate how many intervals fit in a time period</description></item>
    /// <item><description><strong>RatioPer* methods:</strong> Calculate total ratio value for a time period (intervals × ratio)</description></item>
    /// </list>
    /// 
    /// <para>
    /// <strong>Common Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Conversion rates: "Convert 70% of leads every 2 weeks"</description></item>
    /// <item><description>Performance metrics: "Achieve 0.95 efficiency ratio every 8 hours"</description></item>
    /// <item><description>Quality metrics: "Maintain 0.99 success ratio every day"</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: 70% conversion rate every 2 weeks
    /// var conversionInterval = new RatioInterval&lt;double&gt;(0.70, 2, IntervalType.Weeks);
    /// 
    /// // How many 2-week intervals are there per month?
    /// decimal intervalsPerMonth = conversionInterval.PerMonth(); // ~2.17 intervals
    /// 
    /// // What's the total conversion ratio per month?
    /// decimal totalConversionPerMonth = conversionInterval.RatioPerMonth(); // ~1.52 (0.70 × 2.17)
    /// 
    /// // Daily breakdown
    /// decimal totalConversionPerDay = conversionInterval.RatioPerDay(); // ~0.05 (0.70 × 0.071)
    /// </code>
    /// </example>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RatioInterval<T> : Interval<T> where T : IComparable<T>, IConvertible
    {

        #region Properties

        /// <summary>
        /// Gets or sets the decimal ratio value that is calculated over the given interval.
        /// Can represent a ratio, rate, or other decimal value per time period.
        /// </summary>
        public decimal Ratio { get; set; }

        /// <summary>
        /// Returns a string suitable for display in the debugger. Ensures such strings are compiled by the runtime and not interpreted by the currently-executing language.
        /// </summary>
        /// <remarks>http://blogs.msdn.com/b/jaredpar/archive/2011/03/18/debuggerdisplay-attribute-best-practices.aspx</remarks>
        private string DebuggerDisplay => $"Interval: {Value} {Type}, Ratio: {Ratio}";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RatioInterval{T}"/> class with default values.
        /// </summary>
        public RatioInterval()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RatioInterval{T}"/> class with the specified interval value and type.
        /// </summary>
        /// <param name="value">The duration of the interval.</param>
        /// <param name="type">The base unit that describes what the quantity of this interval references.</param>
        public RatioInterval(T value, IntervalType type) : base(value, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RatioInterval{T}"/> class with the specified ratio, interval value, and type.
        /// </summary>
        /// <param name="ratio">The decimal ratio value that is calculated over the given interval.</param>
        /// <param name="value">The duration of the interval.</param>
        /// <param name="type">The base unit that describes what the quantity of this interval references.</param>
        public RatioInterval(decimal ratio, T value, IntervalType type) : this(value, type)
        {
            Ratio = ratio;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the total ratio value per minute based on the interval and ratio.
        /// This method multiplies the interval frequency (how many intervals occur per minute) by the ratio value.
        /// </summary>
        /// <returns>The total ratio value per minute as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 0.5 ratio every 2 hours = 0.5 * (60/120) = 0.25 ratio per minute
        /// var interval = new RatioInterval&lt;double&gt;(0.5, 2, IntervalType.Hours);
        /// decimal ratioPerMinute = interval.RatioPerMinute();
        /// </code>
        /// </example>
        public decimal RatioPerMinute()
        {
            return base.PerMinute() * Ratio;
        }

        /// <summary>
        /// Calculates the total ratio value per minute for a given quantity based on the interval and ratio.
        /// </summary>
        /// <param name="quantity">The quantity to apply the ratio calculation to.</param>
        /// <returns>The total ratio value per minute as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 70% conversion every month, total conversions from 1000 leads per minute
        /// var conversion = new RatioInterval&lt;double&gt;(0.70m, 1, IntervalType.Months);
        /// decimal conversionsPerMinute = conversion.RatioPerMinute(1000); // ~0.016 conversions per minute
        /// </code>
        /// </example>
        public decimal RatioPerMinute(decimal quantity)
        {
            return RatioPerMinute() * quantity;
        }

        /// <summary>
        /// Calculates the total ratio value per hour based on the interval and ratio.
        /// This method multiplies the interval frequency (how many intervals occur per hour) by the ratio value.
        /// </summary>
        /// <returns>The total ratio value per hour as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 0.7 ratio every 1.5 hours = 0.7 * (60/90) = 0.467 ratio per hour
        /// var interval = new RatioInterval&lt;double&gt;(0.7, 1.5, IntervalType.Hours);
        /// decimal ratioPerHour = interval.RatioPerHour();
        /// </code>
        /// </example>
        public decimal RatioPerHour()
        {
            return base.PerHour() * Ratio;
        }

        /// <summary>
        /// Calculates the total ratio value per hour for a given quantity based on the interval and ratio.
        /// </summary>
        /// <param name="quantity">The quantity to apply the ratio calculation to.</param>
        /// <returns>The total ratio value per hour as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 70% conversion every 1.5 hours, total conversions from 100 leads per hour
        /// var conversion = new RatioInterval&lt;double&gt;(0.70m, 1.5, IntervalType.Hours);
        /// decimal conversionsPerHour = conversion.RatioPerHour(100); // ~46.67 conversions per hour
        /// </code>
        /// </example>
        public decimal RatioPerHour(decimal quantity)
        {
            return RatioPerHour() * quantity;
        }

        /// <summary>
        /// Calculates the total ratio value per day based on the interval and ratio.
        /// This method multiplies the interval frequency (how many intervals occur per day) by the ratio value.
        /// </summary>
        /// <returns>The total ratio value per day as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 0.75 ratio every 6 hours = 0.75 * 4 = 3.0 ratio per day
        /// var interval = new RatioInterval&lt;double&gt;(0.75, 6, IntervalType.Hours);
        /// decimal ratioPerDay = interval.RatioPerDay();
        /// </code>
        /// </example>
        public decimal RatioPerDay()
        {
            return base.PerDay() * Ratio;
        }

        /// <summary>
        /// Calculates the total ratio value per day for a given quantity based on the interval and ratio.
        /// </summary>
        /// <param name="quantity">The quantity to apply the ratio calculation to.</param>
        /// <returns>The total ratio value per day as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 70% conversion every month, total conversions from 30 customers per day
        /// var conversion = new RatioInterval&lt;double&gt;(0.70m, 1, IntervalType.Months);
        /// decimal conversionsPerDay = conversion.RatioPerDay(30); // ~0.69 conversions per day
        /// </code>
        /// </example>
        public decimal RatioPerDay(decimal quantity)
        {
            return RatioPerDay() * quantity;
        }

        /// <summary>
        /// Calculates the total ratio value per week based on the interval and ratio.
        /// This method multiplies the interval frequency (how many intervals occur per week) by the ratio value.
        /// </summary>
        /// <returns>The total ratio value per week as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 0.8 ratio every 2 days = 0.8 * 3.5 = 2.8 ratio per week
        /// var interval = new RatioInterval&lt;double&gt;(0.8, 2, IntervalType.Days);
        /// decimal ratioPerWeek = interval.RatioPerWeek();
        /// </code>
        /// </example>
        public decimal RatioPerWeek()
        {
            return base.PerWeek() * Ratio;
        }

        /// <summary>
        /// Calculates the total ratio value per week for a given quantity based on the interval and ratio.
        /// </summary>
        /// <param name="quantity">The quantity to apply the ratio calculation to.</param>
        /// <returns>The total ratio value per week as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 80% conversion every 2 days, total conversions from 50 leads per week
        /// var conversion = new RatioInterval&lt;double&gt;(0.80m, 2, IntervalType.Days);
        /// decimal conversionsPerWeek = conversion.RatioPerWeek(50); // 140 conversions per week
        /// </code>
        /// </example>
        public decimal RatioPerWeek(decimal quantity)
        {
            return RatioPerWeek() * quantity;
        }

        /// <summary>
        /// Calculates the total ratio value per month based on the interval and ratio.
        /// This method multiplies the interval frequency (how many intervals occur per month) by the ratio value.
        /// </summary>
        /// <returns>The total ratio value per month as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 0.6 ratio every 1 week = 0.6 * 4.34 = 2.6 ratio per month
        /// var interval = new RatioInterval&lt;double&gt;(0.6, 1, IntervalType.Weeks);
        /// decimal ratioPerMonth = interval.RatioPerMonth();
        /// </code>
        /// </example>
        public decimal RatioPerMonth()
        {
            return base.PerMonth() * Ratio;
        }

        /// <summary>
        /// Calculates the total ratio value per month for a given quantity based on the interval and ratio.
        /// </summary>
        /// <param name="quantity">The quantity to apply the ratio calculation to.</param>
        /// <returns>The total ratio value per month as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 60% conversion every week, total conversions from 100 leads per month
        /// var conversion = new RatioInterval&lt;double&gt;(0.60m, 1, IntervalType.Weeks);
        /// decimal conversionsPerMonth = conversion.RatioPerMonth(100); // 260 conversions per month
        /// </code>
        /// </example>
        public decimal RatioPerMonth(decimal quantity)
        {
            return RatioPerMonth() * quantity;
        }

        /// <summary>
        /// Calculates the total ratio value per year based on the interval and ratio.
        /// This method multiplies the interval frequency (how many intervals occur per year) by the ratio value.
        /// </summary>
        /// <returns>The total ratio value per year as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 0.9 ratio every 3 months = 0.9 * 4 = 3.6 ratio per year
        /// var interval = new RatioInterval&lt;double&gt;(0.9, 3, IntervalType.Months);
        /// decimal ratioPerYear = interval.RatioPerYear();
        /// </code>
        /// </example>
        public decimal RatioPerYear()
        {
            return base.PerYear() * Ratio;
        }

        /// <summary>
        /// Calculates the total ratio value per year for a given quantity based on the interval and ratio.
        /// </summary>
        /// <param name="quantity">The quantity to apply the ratio calculation to.</param>
        /// <returns>The total ratio value per year as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 90% conversion every 3 months, total conversions from 1000 leads per year
        /// var conversion = new RatioInterval&lt;double&gt;(0.90m, 3, IntervalType.Months);
        /// decimal conversionsPerYear = conversion.RatioPerYear(1000); // 3600 conversions per year
        /// </code>
        /// </example>
        public decimal RatioPerYear(decimal quantity)
        {
            return RatioPerYear() * quantity;
        }

        #endregion

    }
}
