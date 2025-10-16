using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// Represents a percentage rate that occurs at regular time intervals, enabling conversion between different time periods.
    /// This class combines a base time interval (from the <see cref="Interval{T}"/> class) with a percentage rate to calculate 
    /// total percentage amounts across different time periods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Key Concepts:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Value:</strong> Always represents a time duration (e.g., 3 hours, 1.5 days)</description></item>
    /// <item><description><strong>Type:</strong> The time unit for the Value (Hours, Days, Months, etc.)</description></item>
    /// <item><description><strong>Rate:</strong> The decimal percentage rate that occurs each interval period</description></item>
    /// </list>
    /// 
    /// <para>
    /// <strong>Method Types:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Per* methods (inherited):</strong> Calculate how many intervals fit in a time period</description></item>
    /// <item><description><strong>RatePer* methods:</strong> Calculate total percentage rate for a time period (intervals × rate)</description></item>
    /// </list>
    /// 
    /// <para>
    /// <strong>Common Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Interest rates: "Earn 2.5% interest every quarter"</description></item>
    /// <item><description>Growth rates: "Achieve 5% growth every month"</description></item>
    /// <item><description>Error rates: "Allow maximum 0.1% error rate every hour"</description></item>
    /// <item><description>Discount rates: "Apply 10% discount every week"</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example: 2.5% interest rate every quarter (3 months)
    /// var interestInterval = new PercentageInterval&lt;double&gt;(0.025, 3, IntervalType.Months);
    /// 
    /// // How many quarters are there per year?
    /// decimal quartersPerYear = interestInterval.PerYear(); // 4 quarters
    /// 
    /// // What's the total interest rate per year?
    /// decimal totalInterestPerYear = interestInterval.RatePerYear(); // 0.10 (0.025 × 4)
    /// 
    /// // Monthly breakdown
    /// decimal totalInterestPerMonth = interestInterval.RatePerMonth(); // ~0.0083 (0.025 × 0.33)
    /// </code>
    /// </example>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PercentageInterval<T> : Interval<T> where T : IComparable<T>, IConvertible
    {

        #region Properties

        /// <summary>
        /// The amount of money represented by the given <see cref="IntervalType"/>
        /// </summary>
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:P2;(P2);0%}", NullDisplayText = "0%")]
        public decimal Rate { get; set; }

        /// <summary>
        /// Returns a string suitable for display in the debugger. Ensures such strings are compiled by the runtime and not interpreted by the currently-executing language.
        /// </summary>
        /// <remarks>http://blogs.msdn.com/b/jaredpar/archive/2011/03/18/debuggerdisplay-attribute-best-practices.aspx</remarks>
        private string DebuggerDisplay => $"Interval: {Value} {Type}, Rate: {Rate}";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentageInterval{T}"/> class with default values.
        /// </summary>
        public PercentageInterval()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentageInterval{T}"/> class with the specified interval value and type.
        /// </summary>
        /// <param name="value">The duration of the interval.</param>
        /// <param name="type">The base unit that describes what the quantity of this interval references.</param>
        public PercentageInterval(T value, IntervalType type) : base(value, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentageInterval{T}"/> class with the specified rate, interval value, and type.
        /// </summary>
        /// <param name="money">The percentage rate value that is calculated over the given interval.</param>
        /// <param name="value">The duration of the interval.</param>
        /// <param name="type">The base unit that describes what the quantity of this interval references.</param>
        public PercentageInterval(decimal money, T value, IntervalType type) : this(value, type)
        {
            Rate = money;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the total percentage rate per minute based on the interval and rate.
        /// This method multiplies the interval frequency (how many intervals occur per minute) by the rate value.
        /// </summary>
        /// <returns>The total rate value per minute as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 5% rate every 2 hours = 0.05 * (60/120) = 0.025 rate per minute
        /// var interval = new PercentageInterval&lt;double&gt;(0.05, 2, IntervalType.Hours);
        /// decimal ratePerMinute = interval.RatePerMinute();
        /// </code>
        /// </example>
        public decimal RatePerMinute()
        {
            return base.PerMinute() * Rate;
        }

        /// <summary>
        /// Calculates the total percentage rate per minute for a given principal amount based on the interval and rate.
        /// </summary>
        /// <param name="principal">The principal amount to apply the percentage rate to.</param>
        /// <returns>The total rate value per minute as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 2.5% interest every quarter, interest on $50,000 per minute
        /// var interest = new PercentageInterval&lt;double&gt;(0.025m, 3, IntervalType.Months);
        /// decimal interestPerMinute = interest.RatePerMinute(50000); // ~$0.19 per minute
        /// </code>
        /// </example>
        public decimal RatePerMinute(decimal principal)
        {
            return RatePerMinute() * principal;
        }

        /// <summary>
        /// Calculates the total percentage rate per hour based on the interval and rate.
        /// This method multiplies the interval frequency (how many intervals occur per hour) by the rate value.
        /// </summary>
        /// <returns>The total rate value per hour as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 12% rate every 3 hours = 0.12 * (60/180) = 0.04 rate per hour
        /// var interval = new PercentageInterval&lt;double&gt;(0.12, 3, IntervalType.Hours);
        /// decimal ratePerHour = interval.RatePerHour();
        /// </code>
        /// </example>
        public decimal RatePerHour()
        {
            return base.PerHour() * Rate;
        }

        /// <summary>
        /// Calculates the total percentage rate per hour for a given principal amount based on the interval and rate.
        /// </summary>
        /// <param name="principal">The principal amount to apply the percentage rate to.</param>
        /// <returns>The total rate value per hour as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 12% growth rate every 3 hours, growth on $10,000 investment per hour
        /// var growth = new PercentageInterval&lt;double&gt;(0.12m, 3, IntervalType.Hours);
        /// decimal growthPerHour = growth.RatePerHour(10000); // $400 per hour
        /// </code>
        /// </example>
        public decimal RatePerHour(decimal principal)
        {
            return RatePerHour() * principal;
        }

        /// <summary>
        /// Calculates the total percentage rate per day based on the interval and rate.
        /// This method multiplies the interval frequency (how many intervals occur per day) by the rate value.
        /// </summary>
        /// <returns>The total rate value per day as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 8% rate every 6 hours = 0.08 * 4 = 0.32 rate per day
        /// var interval = new PercentageInterval&lt;double&gt;(0.08, 6, IntervalType.Hours);
        /// decimal ratePerDay = interval.RatePerDay();
        /// </code>
        /// </example>
        public decimal RatePerDay()
        {
            return base.PerDay() * Rate;
        }

        /// <summary>
        /// Calculates the total percentage rate per day for a given principal amount based on the interval and rate.
        /// </summary>
        /// <param name="principal">The principal amount to apply the percentage rate to.</param>
        /// <returns>The total rate value per day as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 8% growth rate every 6 hours, growth on $25,000 investment per day
        /// var growth = new PercentageInterval&lt;double&gt;(0.08m, 6, IntervalType.Hours);
        /// decimal growthPerDay = growth.RatePerDay(25000); // $8,000 per day
        /// </code>
        /// </example>
        public decimal RatePerDay(decimal principal)
        {
            return RatePerDay() * principal;
        }

        /// <summary>
        /// Calculates the total percentage rate per week based on the interval and rate.
        /// This method multiplies the interval frequency (how many intervals occur per week) by the rate value.
        /// </summary>
        /// <returns>The total rate value per week as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 15% rate every 2 days = 0.15 * 3.5 = 0.525 rate per week
        /// var interval = new PercentageInterval&lt;double&gt;(0.15, 2, IntervalType.Days);
        /// decimal ratePerWeek = interval.RatePerWeek();
        /// </code>
        /// </example>
        public decimal RatePerWeek()
        {
            return base.PerWeek() * Rate;
        }

        /// <summary>
        /// Calculates the total percentage rate per week for a given principal amount based on the interval and rate.
        /// </summary>
        /// <param name="principal">The principal amount to apply the percentage rate to.</param>
        /// <returns>The total rate value per week as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 15% discount rate every 2 days, discount on $1,000 purchase per week
        /// var discount = new PercentageInterval&lt;double&gt;(0.15m, 2, IntervalType.Days);
        /// decimal discountPerWeek = discount.RatePerWeek(1000); // $525 per week
        /// </code>
        /// </example>
        public decimal RatePerWeek(decimal principal)
        {
            return RatePerWeek() * principal;
        }

        /// <summary>
        /// Calculates the total percentage rate per month based on the interval and rate.
        /// This method multiplies the interval frequency (how many intervals occur per month) by the rate value.
        /// </summary>
        /// <returns>The total rate value per month as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 10% rate every 1 week = 0.10 * 4.34 = 0.434 rate per month
        /// var interval = new PercentageInterval&lt;double&gt;(0.10, 1, IntervalType.Weeks);
        /// decimal ratePerMonth = interval.RatePerMonth();
        /// </code>
        /// </example>
        public decimal RatePerMonth()
        {
            return base.PerMonth() * Rate;
        }

        /// <summary>
        /// Calculates the total percentage rate per month for a given principal amount based on the interval and rate.
        /// </summary>
        /// <param name="principal">The principal amount to apply the percentage rate to.</param>
        /// <returns>The total rate value per month as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 10% growth rate every week, growth on $5,000 investment per month
        /// var growth = new PercentageInterval&lt;double&gt;(0.10m, 1, IntervalType.Weeks);
        /// decimal growthPerMonth = growth.RatePerMonth(5000); // $2,170 per month
        /// </code>
        /// </example>
        public decimal RatePerMonth(decimal principal)
        {
            return RatePerMonth() * principal;
        }

        /// <summary>
        /// Calculates the total percentage rate per year based on the interval and rate.
        /// This method multiplies the interval frequency (how many intervals occur per year) by the rate value.
        /// </summary>
        /// <returns>The total rate value per year as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 20% rate every 3 months = 0.20 * 4 = 0.80 rate per year
        /// var interval = new PercentageInterval&lt;double&gt;(0.20, 3, IntervalType.Months);
        /// decimal ratePerYear = interval.RatePerYear();
        /// </code>
        /// </example>
        public decimal RatePerYear()
        {
            return base.PerYear() * Rate;
        }

        /// <summary>
        /// Calculates the total percentage rate per year for a given principal amount based on the interval and rate.
        /// </summary>
        /// <param name="principal">The principal amount to apply the percentage rate to.</param>
        /// <returns>The total rate value per year as a decimal.</returns>
        /// <example>
        /// <code>
        /// // 20% annual return every 3 months, return on $100,000 investment per year
        /// var returns = new PercentageInterval&lt;double&gt;(0.20m, 3, IntervalType.Months);
        /// decimal returnsPerYear = returns.RatePerYear(100000); // $80,000 per year
        /// </code>
        /// </example>
        public decimal RatePerYear(decimal principal)
        {
            return RatePerYear() * principal;
        }

        #endregion

    }

}
