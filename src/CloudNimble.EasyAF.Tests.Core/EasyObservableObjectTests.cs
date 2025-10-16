using CloudNimble.EasyAF.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace CloudNimble.EasyAF.Tests.Core
{

    [TestClass]
    public class EasyObservableObjectTests
    {

        #region Test Model

        private class TestObservableObject : EasyObservableObject
        {
            private string _name;
            private int _age;
            private bool _isActive;
            private DateTime? _birthDate;

            public string Name
            {
                get => _name;
                set => Set(nameof(Name), ref _name, value);
            }

            public int Age
            {
                get => _age;
                set => Set(() => Age, ref _age, value);
            }

            public bool IsActive
            {
                get => _isActive;
                set => Set(nameof(IsActive), ref _isActive, value);
            }

            public DateTime? BirthDate
            {
                get => _birthDate;
                set => Set(() => BirthDate, ref _birthDate, value);
            }
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_ShouldInitializeSuccessfully()
        {
            var obj = new TestObservableObject();

            obj.Should().NotBeNull();
            obj.Name.Should().BeNull();
            obj.Age.Should().Be(0);
            obj.IsActive.Should().BeFalse();
            obj.BirthDate.Should().BeNull();
        }

        #endregion

        #region PropertyChanged Event Tests

        [TestMethod]
        public void Set_WithStringPropertyName_ShouldRaisePropertyChangedEvent()
        {
            var obj = new TestObservableObject();
            var eventRaised = false;
            string propertyName = null;

            obj.PropertyChanged += (sender, e) =>
            {
                eventRaised = true;
                propertyName = e.PropertyName;
            };

            obj.Name = "John Doe";

            eventRaised.Should().BeTrue();
            propertyName.Should().Be(nameof(TestObservableObject.Name));
            obj.Name.Should().Be("John Doe");
        }

        [TestMethod]
        public void Set_WithExpressionPropertyName_ShouldRaisePropertyChangedEvent()
        {
            var obj = new TestObservableObject();
            var eventRaised = false;
            string propertyName = null;

            obj.PropertyChanged += (sender, e) =>
            {
                eventRaised = true;
                propertyName = e.PropertyName;
            };

            obj.Age = 25;

            eventRaised.Should().BeTrue();
            propertyName.Should().Be(nameof(TestObservableObject.Age));
            obj.Age.Should().Be(25);
        }

        [TestMethod]
        public void Set_WithSameValue_ShouldNotRaisePropertyChangedEvent()
        {
            var obj = new TestObservableObject();
            obj.Name = "John Doe";

            var eventRaised = false;
            obj.PropertyChanged += (sender, e) => eventRaised = true;

            obj.Name = "John Doe"; // Same value

            eventRaised.Should().BeFalse();
        }

        [TestMethod]
        public void Set_WithNullableProperty_ShouldRaisePropertyChangedEvent()
        {
            var obj = new TestObservableObject();
            var eventRaised = false;
            string propertyName = null;

            obj.PropertyChanged += (sender, e) =>
            {
                eventRaised = true;
                propertyName = e.PropertyName;
            };

            var birthDate = new DateTime(1990, 1, 1);
            obj.BirthDate = birthDate;

            eventRaised.Should().BeTrue();
            propertyName.Should().Be(nameof(TestObservableObject.BirthDate));
            obj.BirthDate.Should().Be(birthDate);
        }

        #endregion

        #region RaisePropertyChanged Tests

        [TestMethod]
        public void RaisePropertyChanged_WithNullPropertyName_ShouldThrowNotSupportedException()
        {
            var obj = new TestObservableObject();

            Action act = () => obj.PropertyChangedHandler?.Invoke(obj, new PropertyChangedEventArgs(null));

            // We can't test the protected method directly, but we can test the behavior through Set
            act = () => obj.GetType().GetMethod("RaisePropertyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(string) }, null)
                ?.Invoke(obj, new object[] { null });

            act.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<NotSupportedException>()
                .WithMessage("Raising the PropertyChanged event with an empty string or null is not supported.");
        }

        [TestMethod]
        public void RaisePropertyChanged_WithEmptyPropertyName_ShouldThrowNotSupportedException()
        {
            var obj = new TestObservableObject();

            Action act = () => obj.GetType().GetMethod("RaisePropertyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(string) }, null)
                ?.Invoke(obj, new object[] { "" });

            act.Should().Throw<System.Reflection.TargetInvocationException>()
                .WithInnerException<NotSupportedException>()
                .WithMessage("Raising the PropertyChanged event with an empty string or null is not supported.");
        }

        [TestMethod]
        public void RaisePropertyChanged_WithNullExpression_ShouldNotThrow()
        {
            var obj = new TestObservableObject();
            var eventRaised = false;

            obj.PropertyChanged += (sender, e) => eventRaised = true;

            var method = obj.GetType().GetMethod("RaisePropertyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(Expression<>).MakeGenericType(typeof(Func<>).MakeGenericType(typeof(int))) }, null);
            
            Action act = () => method?.Invoke(obj, new object[] { null });

            act.Should().NotThrow();
            eventRaised.Should().BeFalse();
        }

        #endregion

        #region Clone Tests

        [TestMethod]
        public void Clone_ShouldCreateDeepCopyWithSameValues()
        {
            var original = new TestObservableObject
            {
                Name = "John Doe",
                Age = 25,
                IsActive = true,
                BirthDate = new DateTime(1998, 5, 15)
            };

            var clone = original.Clone<TestObservableObject>();

            clone.Should().NotBeSameAs(original);
            clone.Name.Should().Be(original.Name);
            clone.Age.Should().Be(original.Age);
            clone.IsActive.Should().Be(original.IsActive);
            clone.BirthDate.Should().Be(original.BirthDate);
        }

        [TestMethod]
        public void Clone_WithDefaultValues_ShouldCreateCopyWithDefaults()
        {
            var original = new TestObservableObject();

            var clone = original.Clone<TestObservableObject>();

            clone.Should().NotBeSameAs(original);
            clone.Name.Should().BeNull();
            clone.Age.Should().Be(0);
            clone.IsActive.Should().BeFalse();
            clone.BirthDate.Should().BeNull();
        }

        [TestMethod]
        public void Clone_ChangesToClone_ShouldNotAffectOriginal()
        {
            var original = new TestObservableObject
            {
                Name = "John Doe",
                Age = 25
            };

            var clone = original.Clone<TestObservableObject>();
            clone.Name = "Jane Doe";
            clone.Age = 30;

            original.Name.Should().Be("John Doe");
            original.Age.Should().Be(25);
        }

        #endregion

        #region Dispose Tests

        [TestMethod]
        public void Dispose_ShouldNotThrow()
        {
            var obj = new TestObservableObject();

            Action act = () => obj.Dispose();

            act.Should().NotThrow();
        }

        [TestMethod]
        public void Dispose_CalledMultipleTimes_ShouldNotThrow()
        {
            var obj = new TestObservableObject();

            Action act = () =>
            {
                obj.Dispose();
                obj.Dispose();
                obj.Dispose();
            };

            act.Should().NotThrow();
        }

        #endregion

        #region PropertyChangedHandler Tests

        [TestMethod]
        public void PropertyChangedHandler_ShouldProvideAccessToEvent()
        {
            var obj = new TestObservableObject();
            var eventHandlerCalled = false;

            PropertyChangedEventHandler handler = (sender, e) => eventHandlerCalled = true;
            obj.PropertyChanged += handler;

            obj.PropertyChangedHandler.Should().NotBeNull();
            
            obj.Name = "Test";
            
            eventHandlerCalled.Should().BeTrue();
        }

        [TestMethod]
        public void PropertyChangedHandler_WithNoSubscribers_ShouldBeNull()
        {
            var obj = new TestObservableObject();

            obj.PropertyChangedHandler.Should().BeNull();
        }

        #endregion

    }

}
