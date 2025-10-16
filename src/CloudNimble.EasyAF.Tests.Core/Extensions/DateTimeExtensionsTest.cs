using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CloudNimble.EasyAF.Tests.Core
{

    /// <summary>
    /// A set of tests on the extensions to DateTime
    /// </summary>
    [TestClass]
    public class DateTimeExtensionsTest
    {

        /// <summary>
        /// Tests that the Quarter calculations for <see cref="DateTime"/> are correct when skewed by alternate fiscal years
        /// </summary>
        [TestMethod]
        public void CanCalculateQuarter_DateTime_UsingFiscalYear()
        {
            var targetDate = new DateTime(2021, 1, 1);
            var fiscalYearStart = new DateTime(2021, 1, 1);

            // first month of calendar year for jan-dec fiscal year
            var quarter = targetDate.GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

            // third month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(2).GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

            // fourth month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(3).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // eight month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(7).GetQuarter(fiscalYearStart);
            quarter.Should().Be(3);

            // tenth month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(9).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // eleventh month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(10).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // last month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(11).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // set new fiscal year start
            fiscalYearStart = new DateTime(2021, 7, 1);

            // first month of calendar year for july-june based fiscal year
            quarter = targetDate.GetQuarter(fiscalYearStart);
            quarter.Should().Be(3);

            // third month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(2).GetQuarter(fiscalYearStart);
            quarter.Should().Be(3);

            // fourth month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(3).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // eight month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(7).GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

            // tenth month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(9).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // eleventh month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(10).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // last month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(11).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // set new fiscal year start
            fiscalYearStart = new DateTime(2021, 12, 1);

            // first month of calendar year for dec-nov based fiscal year
            quarter = targetDate.GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

            // third month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(2).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // fourth month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(3).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // eight month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(7).GetQuarter(fiscalYearStart);
            quarter.Should().Be(3);

            // tenth month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(9).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // eleventh month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(10).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // last month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(11).GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

        }

        /// <summary>
        /// Tests that the Quarter calculations for <see cref="DateTime"/> are correct when skewed by alternate fiscal years
        /// </summary>
        [TestMethod]
        public void CanCalculateQuarter_DateTimeOffset_UsingFiscalYear()
        {
            var targetDate = new DateTimeOffset(new DateTime(2021, 1, 1));
            var fiscalYearStart = new DateTimeOffset(new DateTime(2021, 1, 1));

            // first month of calendar year for jan-dec fiscal year
            var quarter = targetDate.GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

            // third month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(2).GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

            // fourth month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(3).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // eight month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(7).GetQuarter(fiscalYearStart);
            quarter.Should().Be(3);

            // tenth month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(9).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // eleventh month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(10).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // last month of calendar year for jan-dec fiscal year
            quarter = targetDate.AddMonths(11).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // set new fiscal year start
            fiscalYearStart = new DateTimeOffset(new DateTime(2021, 7, 1));

            // first month of calendar year for july-june based fiscal year
            quarter = targetDate.GetQuarter(fiscalYearStart);
            quarter.Should().Be(3);

            // third month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(2).GetQuarter(fiscalYearStart);
            quarter.Should().Be(3);

            // fourth month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(3).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // eight month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(7).GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

            // tenth month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(9).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // eleventh month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(10).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // last month of calendar year for july-june fiscal year
            quarter = targetDate.AddMonths(11).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // set new fiscal year start
            fiscalYearStart = new DateTimeOffset(new DateTime(2021, 12, 1));

            // first month of calendar year for dec-nov based fiscal year
            quarter = targetDate.GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

            // third month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(2).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // fourth month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(3).GetQuarter(fiscalYearStart);
            quarter.Should().Be(2);

            // eight month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(7).GetQuarter(fiscalYearStart);
            quarter.Should().Be(3);

            // tenth month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(9).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // eleventh month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(10).GetQuarter(fiscalYearStart);
            quarter.Should().Be(4);

            // last month of calendar year for dec-nov fiscal year
            quarter = targetDate.AddMonths(11).GetQuarter(fiscalYearStart);
            quarter.Should().Be(1);

        }

    }

}
