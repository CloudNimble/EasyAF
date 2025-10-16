using CloudNimble.EasyAF.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace CloudNimble.EasyAF.Tests.Core
{

    [TestClass]
    public class RatioIntervalTests
    {

        #region ClassInitialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_ShouldInitializeWithDefaults()
        {
            var interval = new RatioInterval<int>();

            interval.Value.Should().Be(0);
            interval.Type.Should().Be(IntervalType.Months);
            interval.Ratio.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithValueAndType_ShouldInitializeProperties()
        {
            var value = 3;
            var type = IntervalType.Months;

            var interval = new RatioInterval<int>(value, type);

            interval.Value.Should().Be(value);
            interval.Type.Should().Be(type);
            interval.Ratio.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithRatioValueAndType_ShouldInitializeAllProperties()
        {
            var ratio = 0.75m;
            var value = 6;
            var type = IntervalType.Hours;

            var interval = new RatioInterval<int>(ratio, value, type);

            interval.Value.Should().Be(value);
            interval.Type.Should().Be(type);
            interval.Ratio.Should().Be(ratio);
        }

        #endregion

        #region RatioPerMinute Tests

        [TestMethod]
        [DataRow(1D, IntervalType.Minutes, 1D)]
        [DataRow(1.5D, IntervalType.Minutes, 1.5D)]
        [DataRow(2D, IntervalType.Minutes, 2D)]
        [DataRow(3D, IntervalType.Minutes, 3D)]
        [DataRow(4D, IntervalType.Minutes, 4D)]

        [DataRow(1D, IntervalType.Hours, 0.01666667D)]
        [DataRow(1.5D, IntervalType.Hours, 0.025D)]
        [DataRow(2D, IntervalType.Hours, 0.03333333D)]
        [DataRow(3D, IntervalType.Hours, 0.05D)]
        [DataRow(4D, IntervalType.Hours, 0.06666667D)]

        [DataRow(1D, IntervalType.Days, 0.00069444D)]
        [DataRow(1.5D, IntervalType.Days, 0.00104167D)]
        [DataRow(2D, IntervalType.Days, 0.00138889D)]
        [DataRow(3D, IntervalType.Days, 0.00208333D)]
        [DataRow(4D, IntervalType.Days, 0.00277778D)]

        [DataRow(1D, IntervalType.Weeks, 0.00009921D)]
        [DataRow(1.5D, IntervalType.Weeks, 0.00014881D)]
        [DataRow(2D, IntervalType.Weeks, 0.00019841D)]
        [DataRow(3D, IntervalType.Weeks, 0.00029762D)]
        [DataRow(4D, IntervalType.Weeks, 0.00039683D)]

        [DataRow(1D, IntervalType.Months, 0.00002283D)]
        [DataRow(1.5D, IntervalType.Months, 0.00003425D)]
        [DataRow(2D, IntervalType.Months, 0.00004566D)]
        [DataRow(3D, IntervalType.Months, 0.00006849D)]
        [DataRow(4D, IntervalType.Months, 0.00009132D)]

        [DataRow(1D, IntervalType.Years, 0.0000019D)]
        [DataRow(1.5D, IntervalType.Years, 0.00000285D)]
        [DataRow(2D, IntervalType.Years, 0.00000381D)]
        [DataRow(3D, IntervalType.Years, 0.00000571D)]
        [DataRow(4D, IntervalType.Years, 0.00000761D)]
        public void RatioPerMinute_ShouldReturnCorrectValue(double intervalValue, IntervalType intervalType, double expected)
        {
            var interval = new RatioInterval<double>(1.0m, intervalValue, intervalType);

            var result = interval.RatioPerMinute();

            result.Should().BeApproximately((decimal)expected, 0.0000001m);
        }

        #endregion

        #region RatioPerHour Tests

        [TestMethod]
        [DataRow(1D, IntervalType.Minutes, 60D)]
        [DataRow(1.5D, IntervalType.Minutes, 40D)]
        [DataRow(2D, IntervalType.Minutes, 30D)]
        [DataRow(3D, IntervalType.Minutes, 20D)]
        [DataRow(4D, IntervalType.Minutes, 15D)]

        [DataRow(1D, IntervalType.Hours, 1D)]
        [DataRow(1.5D, IntervalType.Hours, 1.5D)]
        [DataRow(2D, IntervalType.Hours, 2D)]
        [DataRow(3D, IntervalType.Hours, 3D)]
        [DataRow(4D, IntervalType.Hours, 4D)]

        [DataRow(1D, IntervalType.Days, 0.04166667D)]
        [DataRow(1.5D, IntervalType.Days, 0.0625D)]
        [DataRow(2D, IntervalType.Days, 0.08333333D)]
        [DataRow(3D, IntervalType.Days, 0.125D)]
        [DataRow(4D, IntervalType.Days, 0.16666667D)]

        [DataRow(1D, IntervalType.Weeks, 0.00595238D)]
        [DataRow(1.5D, IntervalType.Weeks, 0.00892857D)]
        [DataRow(2D, IntervalType.Weeks, 0.01190476D)]
        [DataRow(3D, IntervalType.Weeks, 0.01785714D)]
        [DataRow(4D, IntervalType.Weeks, 0.02380952D)]

        [DataRow(1D, IntervalType.Months, 0.00136986D)]
        [DataRow(1.5D, IntervalType.Months, 0.00205479D)]
        [DataRow(2D, IntervalType.Months, 0.00273973D)]
        [DataRow(3D, IntervalType.Months, 0.00410959D)]
        [DataRow(4D, IntervalType.Months, 0.00547945D)]

        [DataRow(1D, IntervalType.Years, 0.00011416D)]
        [DataRow(1.5D, IntervalType.Years, 0.00017123D)]
        [DataRow(2D, IntervalType.Years, 0.00022831D)]
        [DataRow(3D, IntervalType.Years, 0.00034247D)]
        [DataRow(4D, IntervalType.Years, 0.00045662D)]
        public void RatioPerHour_ShouldReturnCorrectValue(double intervalValue, IntervalType intervalType, double expected)
        {
            var interval = new RatioInterval<double>(1.0m, intervalValue, intervalType);

            var result = interval.RatioPerHour();

            result.Should().BeApproximately((decimal)expected, 0.00000001m);
        }

        #endregion

        #region RatioPerDay Tests

        [TestMethod]
        [DataRow(1D, IntervalType.Minutes, 1440D)]
        [DataRow(1.5D, IntervalType.Minutes, 960D)]
        [DataRow(2D, IntervalType.Minutes, 720D)]
        [DataRow(3D, IntervalType.Minutes, 480D)]
        [DataRow(4D, IntervalType.Minutes, 360D)]

        [DataRow(1D, IntervalType.Hours, 24D)]
        [DataRow(1.5D, IntervalType.Hours, 16D)]
        [DataRow(2D, IntervalType.Hours, 12D)]
        [DataRow(3D, IntervalType.Hours, 8D)]
        [DataRow(4D, IntervalType.Hours, 6D)]

        [DataRow(1D, IntervalType.Days, 1D)]
        [DataRow(1.5D, IntervalType.Days, 1.5D)]
        [DataRow(2D, IntervalType.Days, 2D)]
        [DataRow(3D, IntervalType.Days, 3D)]
        [DataRow(4D, IntervalType.Days, 4D)]

        [DataRow(1D, IntervalType.Weeks, 0.14285714D)]
        [DataRow(1.5D, IntervalType.Weeks, 0.21428571D)]
        [DataRow(2D, IntervalType.Weeks, 0.28571429D)]
        [DataRow(3D, IntervalType.Weeks, 0.42857143D)]
        [DataRow(4D, IntervalType.Weeks, 0.57142857D)]

        [DataRow(1D, IntervalType.Months, 0.03287671D)]
        [DataRow(1.5D, IntervalType.Months, 0.04931507D)]
        [DataRow(2D, IntervalType.Months, 0.06575342D)]
        [DataRow(3D, IntervalType.Months, 0.09863014D)]
        [DataRow(4D, IntervalType.Months, 0.13150685D)]

        [DataRow(1D, IntervalType.Years, 0.00273973D)]
        [DataRow(1.5D, IntervalType.Years, 0.00410959D)]
        [DataRow(2D, IntervalType.Years, 0.00547945D)]
        [DataRow(3D, IntervalType.Years, 0.00821918D)]
        [DataRow(4D, IntervalType.Years, 0.0109589D)]
        public void RatioPerDay_ShouldReturnCorrectValue(double intervalValue, IntervalType intervalType, double expected)
        {
            var interval = new RatioInterval<double>(1.0m, intervalValue, intervalType);

            var result = interval.RatioPerDay();

            result.Should().BeApproximately((decimal)expected, 0.00000001m);
        }

        #endregion

        #region RatioPerWeek Tests

        [TestMethod]
        [DataRow(1D, IntervalType.Minutes, 10080D)]
        [DataRow(1.5D, IntervalType.Minutes, 6720D)]
        [DataRow(2D, IntervalType.Minutes, 5040D)]
        [DataRow(3D, IntervalType.Minutes, 3360D)]
        [DataRow(4D, IntervalType.Minutes, 2520D)]

        [DataRow(1D, IntervalType.Hours, 168D)]
        [DataRow(1.5D, IntervalType.Hours, 112D)]
        [DataRow(2D, IntervalType.Hours, 84D)]
        [DataRow(3D, IntervalType.Hours, 56D)]
        [DataRow(4D, IntervalType.Hours, 42D)]

        [DataRow(1D, IntervalType.Days, 7D)]
        [DataRow(1.5D, IntervalType.Days, 4.66666667D)]
        [DataRow(2D, IntervalType.Days, 3.5D)]
        [DataRow(3D, IntervalType.Days, 2.33333333D)]
        [DataRow(4D, IntervalType.Days, 1.75D)]

        [DataRow(1D, IntervalType.Weeks, 1D)]
        [DataRow(1.5D, IntervalType.Weeks, 1.5D)]
        [DataRow(2D, IntervalType.Weeks, 2D)]
        [DataRow(3D, IntervalType.Weeks, 3D)]
        [DataRow(4D, IntervalType.Weeks, 4D)]

        [DataRow(1D, IntervalType.Months, 0.23013699D)]
        [DataRow(1.5D, IntervalType.Months, 0.34520548D)]
        [DataRow(2D, IntervalType.Months, 0.46027397D)]
        [DataRow(3D, IntervalType.Months, 0.69041096D)]
        [DataRow(4D, IntervalType.Months, 0.92054794D)]

        [DataRow(1D, IntervalType.Years, 0.01917808D)]
        [DataRow(1.5D, IntervalType.Years, 0.02876712D)]
        [DataRow(2D, IntervalType.Years, 0.03835616D)]
        [DataRow(3D, IntervalType.Years, 0.05753425D)]
        [DataRow(4D, IntervalType.Years, 0.07671233D)]
        public void RatioPerWeek_ShouldReturnCorrectValue(double intervalValue, IntervalType intervalType, double expected)
        {
            var interval = new RatioInterval<double>(1.0m, intervalValue, intervalType);

            var result = interval.RatioPerWeek();

            result.Should().BeApproximately((decimal)expected, 0.00000001m);
        }

        #endregion

        #region RatioPerMonth Tests

        [TestMethod]
        [DataRow(1D, IntervalType.Minutes, 43800D)]
        [DataRow(1.5D, IntervalType.Minutes, 29200D)]
        [DataRow(2D, IntervalType.Minutes, 21900D)]
        [DataRow(3D, IntervalType.Minutes, 14600D)]
        [DataRow(4D, IntervalType.Minutes, 10950D)]

        [DataRow(1D, IntervalType.Hours, 730D)]
        [DataRow(1.5D, IntervalType.Hours, 486.66666667D)]
        [DataRow(2D, IntervalType.Hours, 365D)]
        [DataRow(3D, IntervalType.Hours, 243.33333333D)]
        [DataRow(4D, IntervalType.Hours, 182.5D)]

        [DataRow(1D, IntervalType.Days, 30D)]
        [DataRow(1.5D, IntervalType.Days, 20D)]
        [DataRow(2D, IntervalType.Days, 15D)]
        [DataRow(3D, IntervalType.Days, 10D)]
        [DataRow(4D, IntervalType.Days, 7.5D)]

        [DataRow(1D, IntervalType.Weeks, 4.3452381D)]
        [DataRow(1.5D, IntervalType.Weeks, 2.8968254D)]
        [DataRow(2D, IntervalType.Weeks, 2.17261905D)]
        [DataRow(3D, IntervalType.Weeks, 1.4484127D)]
        [DataRow(4D, IntervalType.Weeks, 1.08630952D)]

        [DataRow(1D, IntervalType.Months, 1D)]
        [DataRow(1.5D, IntervalType.Months, 1.5D)]
        [DataRow(2D, IntervalType.Months, 2D)]
        [DataRow(3D, IntervalType.Months, 3D)]
        [DataRow(4D, IntervalType.Months, 4D)]

        [DataRow(1D, IntervalType.Years, 0.08333333D)]
        [DataRow(1.5D, IntervalType.Years, 0.125D)]
        [DataRow(2D, IntervalType.Years, 0.16666667D)]
        [DataRow(3D, IntervalType.Years, 0.25D)]
        [DataRow(4D, IntervalType.Years, 0.33333333D)]
        public void RatioPerMonth_ShouldReturnCorrectValue(double intervalValue, IntervalType intervalType, double expected)
        {
            var interval = new RatioInterval<double>(1.0m, intervalValue, intervalType);

            var result = interval.RatioPerMonth();

            result.Should().BeApproximately((decimal)expected, 0.00000001m);
        }

        #endregion

        #region RatioPerYear Tests

        [TestMethod]
        [DataRow(1D, IntervalType.Minutes, 525600D)]
        [DataRow(1.5D, IntervalType.Minutes, 350400D)]
        [DataRow(2D, IntervalType.Minutes, 262800D)]
        [DataRow(3D, IntervalType.Minutes, 175200D)]
        [DataRow(4D, IntervalType.Minutes, 131400D)]

        [DataRow(1D, IntervalType.Hours, 8760D)]
        [DataRow(1.5D, IntervalType.Hours, 5840D)]
        [DataRow(2D, IntervalType.Hours, 4380D)]
        [DataRow(3D, IntervalType.Hours, 2920D)]
        [DataRow(4D, IntervalType.Hours, 2190D)]

        [DataRow(1D, IntervalType.Days, 365D)]
        [DataRow(1.5D, IntervalType.Days, 243.33333333D)]
        [DataRow(2D, IntervalType.Days, 182.5D)]
        [DataRow(3D, IntervalType.Days, 121.66666667D)]
        [DataRow(4D, IntervalType.Days, 91.25D)]

        [DataRow(1D, IntervalType.Weeks, 52.1428571D)]
        [DataRow(1.5D, IntervalType.Weeks, 34.76190473D)]
        [DataRow(2D, IntervalType.Weeks, 26.07142855D)]
        [DataRow(3D, IntervalType.Weeks, 17.38095237D)]
        [DataRow(4D, IntervalType.Weeks, 13.03571428D)]

        [DataRow(1D, IntervalType.Months, 12D)]
        [DataRow(1.5D, IntervalType.Months, 8D)]
        [DataRow(2D, IntervalType.Months, 6D)]
        [DataRow(3D, IntervalType.Months, 4D)]
        [DataRow(4D, IntervalType.Months, 3D)]

        [DataRow(1D, IntervalType.Years, 1D)]
        [DataRow(1.5D, IntervalType.Years, 1.5D)]
        [DataRow(2D, IntervalType.Years, 2D)]
        [DataRow(3D, IntervalType.Years, 3D)]
        [DataRow(4D, IntervalType.Years, 4D)]
        public void RatioPerYear_ShouldReturnCorrectValue(double intervalValue, IntervalType intervalType, double expected)
        {
            var interval = new RatioInterval<double>(1.0m, intervalValue, intervalType);

            var result = interval.RatioPerYear();

            result.Should().BeApproximately((decimal)expected, 0.0000001m);
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_WithDefaultValues_ShouldReturnDefaultFormat()
        {
            var interval = new RatioInterval<int>();
            interval.ToString().Should().Be("0 Months");
        }

        [TestMethod]
        public void ToString_WithCustomValues_ShouldReturnFormattedString()
        {
            var interval = new RatioInterval<int>();
            interval.Value = 3;
            interval.Type = IntervalType.Hours;
            interval.ToString().Should().Be("3 Hours");
        }

        [TestMethod]
        public void ToString_WithSingularValue_ShouldReturnSingularForm()
        {
            var interval = new RatioInterval<int>();
            interval.Value = 1;
            interval.Type = IntervalType.Hours;
            interval.ToString().Should().Be("1 Hour");
        }

        [TestMethod]
        public void ToString_WithDecimalValue_ShouldFormatCorrectly()
        {
            var interval = new RatioInterval<int>(0.75m, 6, IntervalType.Hours);

            interval.ToString().Should().Be("6 Hours");
        }

        #endregion

    }

}
