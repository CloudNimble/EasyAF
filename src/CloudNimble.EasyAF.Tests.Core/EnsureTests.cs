using CloudNimble.EasyAF.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.Tests.Core
{

    [TestClass]
    public class EnsureTests
    {

        #region ArgumentNotNull Tests

        [TestMethod]
        public void ArgumentNotNull_WithValidObject_ShouldNotThrow()
        {
            var testObject = new object();
            var testString = "test";
            var testList = new List<string>();

            Action act1 = () => Ensure.ArgumentNotNull(testObject, nameof(testObject));
            Action act2 = () => Ensure.ArgumentNotNull(testString, nameof(testString));
            Action act3 = () => Ensure.ArgumentNotNull(testList, nameof(testList));

            act1.Should().NotThrow();
            act2.Should().NotThrow();
            act3.Should().NotThrow();
        }

        [TestMethod]
        public void ArgumentNotNull_WithNullObject_ShouldThrowArgumentNullException()
        {
            object testObject = null;
            var argumentName = "testObject";

            Action act = () => Ensure.ArgumentNotNull(testObject, argumentName);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName(argumentName);
        }

        [TestMethod]
        public void ArgumentNotNull_WithNullString_ShouldThrowArgumentNullException()
        {
            string testString = null;
            var argumentName = "testString";

            Action act = () => Ensure.ArgumentNotNull(testString, argumentName);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName(argumentName);
        }

        [TestMethod]
        public void ArgumentNotNull_WithNullCollection_ShouldThrowArgumentNullException()
        {
            List<string> testList = null;
            var argumentName = "testList";

            Action act = () => Ensure.ArgumentNotNull(testList, argumentName);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName(argumentName);
        }

        [TestMethod]
        public void ArgumentNotNull_WithEmptyString_ShouldNotThrow()
        {
            var testString = "";
            var argumentName = "testString";

            Action act = () => Ensure.ArgumentNotNull(testString, argumentName);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void ArgumentNotNull_WithWhitespaceString_ShouldNotThrow()
        {
            var testString = "   ";
            var argumentName = "testString";

            Action act = () => Ensure.ArgumentNotNull(testString, argumentName);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void ArgumentNotNull_WithEmptyCollection_ShouldNotThrow()
        {
            var testList = new List<string>();
            var argumentName = "testList";

            Action act = () => Ensure.ArgumentNotNull(testList, argumentName);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void ArgumentNotNull_WithValueType_ShouldNotThrow()
        {
            var testInt = 42;
            var testBool = true;
            var testDateTime = DateTime.Now;
            var testGuid = Guid.NewGuid();

            Action act1 = () => Ensure.ArgumentNotNull(testInt, nameof(testInt));
            Action act2 = () => Ensure.ArgumentNotNull(testBool, nameof(testBool));
            Action act3 = () => Ensure.ArgumentNotNull(testDateTime, nameof(testDateTime));
            Action act4 = () => Ensure.ArgumentNotNull(testGuid, nameof(testGuid));

            act1.Should().NotThrow();
            act2.Should().NotThrow();
            act3.Should().NotThrow();
            act4.Should().NotThrow();
        }

        [TestMethod]
        public void ArgumentNotNull_WithNullableValueTypeNotNull_ShouldNotThrow()
        {
            int? testInt = 42;
            bool? testBool = true;
            DateTime? testDateTime = DateTime.Now;
            Guid? testGuid = Guid.NewGuid();

            Action act1 = () => Ensure.ArgumentNotNull(testInt, nameof(testInt));
            Action act2 = () => Ensure.ArgumentNotNull(testBool, nameof(testBool));
            Action act3 = () => Ensure.ArgumentNotNull(testDateTime, nameof(testDateTime));
            Action act4 = () => Ensure.ArgumentNotNull(testGuid, nameof(testGuid));

            act1.Should().NotThrow();
            act2.Should().NotThrow();
            act3.Should().NotThrow();
            act4.Should().NotThrow();
        }

        [TestMethod]
        public void ArgumentNotNull_WithNullableValueTypeNull_ShouldThrowArgumentNullException()
        {
            int? testInt = null;
            bool? testBool = null;
            DateTime? testDateTime = null;
            Guid? testGuid = null;

            Action act1 = () => Ensure.ArgumentNotNull(testInt, nameof(testInt));
            Action act2 = () => Ensure.ArgumentNotNull(testBool, nameof(testBool));
            Action act3 = () => Ensure.ArgumentNotNull(testDateTime, nameof(testDateTime));
            Action act4 = () => Ensure.ArgumentNotNull(testGuid, nameof(testGuid));

            act1.Should().Throw<ArgumentNullException>().WithParameterName(nameof(testInt));
            act2.Should().Throw<ArgumentNullException>().WithParameterName(nameof(testBool));
            act3.Should().Throw<ArgumentNullException>().WithParameterName(nameof(testDateTime));
            act4.Should().Throw<ArgumentNullException>().WithParameterName(nameof(testGuid));
        }

        [TestMethod]
        public void ArgumentNotNull_WithNullArgumentName_ShouldStillThrowWithNullParameterName()
        {
            object testObject = null;
            string argumentName = null;

            Action act = () => Ensure.ArgumentNotNull(testObject, argumentName);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName(argumentName);
        }

        [TestMethod]
        public void ArgumentNotNull_WithEmptyArgumentName_ShouldThrowWithEmptyParameterName()
        {
            object testObject = null;
            var argumentName = "";

            Action act = () => Ensure.ArgumentNotNull(testObject, argumentName);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName(argumentName);
        }

        [TestMethod]
        public void ArgumentNotNull_WithMultipleNullObjects_ShouldThrowForEach()
        {
            object obj1 = null;
            object obj2 = null;
            object obj3 = null;

            Action act1 = () => Ensure.ArgumentNotNull(obj1, nameof(obj1));
            Action act2 = () => Ensure.ArgumentNotNull(obj2, nameof(obj2));
            Action act3 = () => Ensure.ArgumentNotNull(obj3, nameof(obj3));

            act1.Should().Throw<ArgumentNullException>().WithParameterName(nameof(obj1));
            act2.Should().Throw<ArgumentNullException>().WithParameterName(nameof(obj2));
            act3.Should().Throw<ArgumentNullException>().WithParameterName(nameof(obj3));
        }

        #endregion

        #region Conditional Compilation Tests

        [TestMethod]
        public void ArgumentNotNull_ShouldUseAppropriateImplementationBasedOnTargetFramework()
        {
            // This test verifies that the method works correctly regardless of which conditional compilation path is taken
            string validString = "test";
            string nullString = null;

            Action validAct = () => Ensure.ArgumentNotNull(validString, nameof(validString));
            Action nullAct = () => Ensure.ArgumentNotNull(nullString, nameof(nullString));

            validAct.Should().NotThrow();
            nullAct.Should().Throw<ArgumentNullException>().WithParameterName(nameof(nullString));
        }

        #endregion

    }

}