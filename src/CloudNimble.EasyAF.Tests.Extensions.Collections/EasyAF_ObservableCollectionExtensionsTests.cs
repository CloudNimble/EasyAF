using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.Extensions.Collections
{

    [TestClass]
    public class EasyAF_ObservableCollectionExtensionsTests
    {

        #region AddRange Tests

        [TestMethod]
        public void AddRange_AddsItems_CountIsCorrect()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            collection.AddRange(new[] { 4, 5, 6 });

            collection.Should().HaveCount(6);
            collection.Should().ContainInOrder(1, 2, 3, 4, 5, 6);
        }

        [TestMethod]
        public void AddRange_EmptyCollection_NoEventFired()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };
            var eventCount = 0;
            collection.CollectionChanged += (s, e) => eventCount++;

            collection.AddRange(Array.Empty<int>());

            eventCount.Should().Be(0);
            collection.Should().HaveCount(3);
        }

        [TestMethod]
        public void AddRange_Null_ThrowsArgumentNullException()
        {
            var collection = new ObservableCollection<int>();

            Action act = () => collection.AddRange(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AddRange_Batched_FiresSingleAddAction()
        {
            var collection = new ObservableCollection<int> { 1 };
            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (s, e) => events.Add(e);

            collection.AddRange(new[] { 2, 3, 4 });

            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Add);
            events[0].NewItems.Cast<int>().Should().HaveCount(3);
            events[0].NewItems.Cast<int>().Should().Contain(new[] { 2, 3, 4 });
            events[0].NewStartingIndex.Should().Be(1);
        }

        [TestMethod]
        public void AddRange_Reset_FiresSingleResetAction()
        {
            var collection = new ObservableCollection<int> { 1 };
            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (s, e) => events.Add(e);

            collection.AddRange(new[] { 2, 3, 4 }, CollectionChangeNotificationMode.Reset);

            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Reset);
            collection.Should().HaveCount(4);
        }

        #endregion

        #region InsertRange Tests

        [TestMethod]
        public void InsertRange_AtBeginning_InsertsCorrectly()
        {
            var collection = new ObservableCollection<string> { "c", "d" };

            collection.InsertRange(0, new[] { "a", "b" });

            collection.Should().ContainInOrder("a", "b", "c", "d");
        }

        [TestMethod]
        public void InsertRange_AtMiddle_InsertsCorrectly()
        {
            var collection = new ObservableCollection<string> { "a", "d" };

            collection.InsertRange(1, new[] { "b", "c" });

            collection.Should().ContainInOrder("a", "b", "c", "d");
        }

        [TestMethod]
        public void InsertRange_AtEnd_InsertsCorrectly()
        {
            var collection = new ObservableCollection<string> { "a", "b" };

            collection.InsertRange(2, new[] { "c", "d" });

            collection.Should().ContainInOrder("a", "b", "c", "d");
        }

        [TestMethod]
        public void InsertRange_NegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            Action act = () => collection.InsertRange(-1, new[] { 4 });

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void InsertRange_IndexBeyondCount_ThrowsArgumentOutOfRangeException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            Action act = () => collection.InsertRange(4, new[] { 4 });

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void InsertRange_Batched_FiresSingleAddActionWithCorrectIndex()
        {
            var collection = new ObservableCollection<int> { 1, 4 };
            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (s, e) => events.Add(e);

            collection.InsertRange(1, new[] { 2, 3 });

            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Add);
            events[0].NewItems.Cast<int>().Should().Contain(new[] { 2, 3 });
            events[0].NewStartingIndex.Should().Be(1);
        }

        #endregion

        #region RemoveRange Tests

        [TestMethod]
        public void RemoveRange_RemovesCorrectItems()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };

            collection.RemoveRange(1, 3);

            collection.Should().HaveCount(2);
            collection.Should().ContainInOrder(1, 5);
        }

        [TestMethod]
        public void RemoveRange_EmptyCount_NoEventFired()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };
            var eventCount = 0;
            collection.CollectionChanged += (s, e) => eventCount++;

            collection.RemoveRange(0, 0);

            eventCount.Should().Be(0);
            collection.Should().HaveCount(3);
        }

        [TestMethod]
        public void RemoveRange_NegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            Action act = () => collection.RemoveRange(-1, 1);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void RemoveRange_NegativeCount_ThrowsArgumentOutOfRangeException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            Action act = () => collection.RemoveRange(0, -1);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void RemoveRange_IndexPlusCountExceedsCollection_ThrowsArgumentException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            Action act = () => collection.RemoveRange(1, 3);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void RemoveRange_Batched_FiresSingleRemoveAction()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (s, e) => events.Add(e);

            collection.RemoveRange(1, 2);

            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Remove);
            events[0].OldItems.Cast<int>().Should().HaveCount(2);
            events[0].OldItems.Cast<int>().Should().Contain(new[] { 2, 3 });
            events[0].OldStartingIndex.Should().Be(1);
        }

        [TestMethod]
        public void RemoveRange_Reset_FiresSingleResetAction()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (s, e) => events.Add(e);

            collection.RemoveRange(1, 2, CollectionChangeNotificationMode.Reset);

            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Reset);
            collection.Should().HaveCount(3);
        }

        #endregion

        #region ReplaceRange Tests

        [TestMethod]
        public void ReplaceRange_ReplacesWithSameCount()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };

            collection.ReplaceRange(1, 2, new[] { 20, 30 });

            collection.Should().HaveCount(5);
            collection.Should().ContainInOrder(1, 20, 30, 4, 5);
        }

        [TestMethod]
        public void ReplaceRange_ReplacesWithFewerItems()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };

            collection.ReplaceRange(1, 3, new[] { 99 });

            collection.Should().HaveCount(3);
            collection.Should().ContainInOrder(1, 99, 5);
        }

        [TestMethod]
        public void ReplaceRange_ReplacesWithMoreItems()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            collection.ReplaceRange(1, 1, new[] { 20, 30, 40 });

            collection.Should().HaveCount(5);
            collection.Should().ContainInOrder(1, 20, 30, 40, 3);
        }

        [TestMethod]
        public void ReplaceRange_Null_ThrowsArgumentNullException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            Action act = () => collection.ReplaceRange(0, 1, null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ReplaceRange_InvalidRange_ThrowsArgumentException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };

            Action act = () => collection.ReplaceRange(1, 3, new[] { 99 });

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ReplaceRange_Batched_FiresSingleReplaceAction()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (s, e) => events.Add(e);

            collection.ReplaceRange(1, 2, new[] { 20, 30 });

            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Replace);
            events[0].NewItems.Cast<int>().Should().Contain(new[] { 20, 30 });
            events[0].OldItems.Cast<int>().Should().Contain(new[] { 2, 3 });
            events[0].NewStartingIndex.Should().Be(1);
        }

        [TestMethod]
        public void ReplaceRange_Reset_FiresSingleResetAction()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (s, e) => events.Add(e);

            collection.ReplaceRange(1, 2, new[] { 20, 30 }, CollectionChangeNotificationMode.Reset);

            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Reset);
        }

        #endregion

        #region PropertyChanged Tests

        [TestMethod]
        public void AddRange_FiresCountAndItemPropertyChanged()
        {
            var collection = new ObservableCollection<int> { 1 };
            var propertyNames = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);

            collection.AddRange(new[] { 2, 3 });

            propertyNames.Should().Contain("Count");
            propertyNames.Should().Contain("Item[]");
        }

        [TestMethod]
        public void RemoveRange_FiresCountAndItemPropertyChanged()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };
            var propertyNames = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);

            collection.RemoveRange(0, 2);

            propertyNames.Should().Contain("Count");
            propertyNames.Should().Contain("Item[]");
        }

        [TestMethod]
        public void ReplaceRange_SameCount_FiresItemButNotCountPropertyChanged()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };
            var propertyNames = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);

            collection.ReplaceRange(0, 2, new[] { 10, 20 });

            propertyNames.Should().NotContain("Count");
            propertyNames.Should().Contain("Item[]");
        }

        [TestMethod]
        public void ReplaceRange_DifferentCount_FiresCountAndItemPropertyChanged()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };
            var propertyNames = new List<string>();
            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);

            collection.ReplaceRange(0, 2, new[] { 10 });

            propertyNames.Should().Contain("Count");
            propertyNames.Should().Contain("Item[]");
        }

        #endregion

        #region Collection<T> Extension Tests

        [TestMethod]
        public void Collection_AddRange_AddsItems()
        {
            Collection<int> collection = new Collection<int> { 1, 2, 3 };

            collection.AddRange(new[] { 4, 5, 6 });

            collection.Should().HaveCount(6);
            collection.Should().ContainInOrder(1, 2, 3, 4, 5, 6);
        }

        [TestMethod]
        public void Collection_InsertRange_InsertsAtIndex()
        {
            Collection<string> collection = new Collection<string> { "a", "d" };

            collection.InsertRange(1, new[] { "b", "c" });

            collection.Should().ContainInOrder("a", "b", "c", "d");
        }

        [TestMethod]
        public void Collection_RemoveRange_RemovesItems()
        {
            Collection<int> collection = new Collection<int> { 1, 2, 3, 4, 5 };

            collection.RemoveRange(1, 3);

            collection.Should().HaveCount(2);
            collection.Should().ContainInOrder(1, 5);
        }

        [TestMethod]
        public void Collection_ReplaceRange_ReplacesItems()
        {
            Collection<int> collection = new Collection<int> { 1, 2, 3, 4, 5 };

            collection.ReplaceRange(1, 2, new[] { 20, 30, 40 });

            collection.Should().HaveCount(6);
            collection.Should().ContainInOrder(1, 20, 30, 40, 4, 5);
        }

        [TestMethod]
        public void Collection_AddRange_Null_ThrowsArgumentNullException()
        {
            Collection<int> collection = new Collection<int>();

            Action act = () => collection.AddRange(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Collection_RemoveRange_BoundsValidation()
        {
            Collection<int> collection = new Collection<int> { 1, 2, 3 };

            Action negativeIndex = () => collection.RemoveRange(-1, 1);
            Action negativeCount = () => collection.RemoveRange(0, -1);
            Action exceedsCount = () => collection.RemoveRange(1, 3);

            negativeIndex.Should().Throw<ArgumentOutOfRangeException>();
            negativeCount.Should().Throw<ArgumentOutOfRangeException>();
            exceedsCount.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ObservableCollection_AddRange_UsesBatchedVersion()
        {
            // When the static type is ObservableCollection<T>, the OverloadResolutionPriority(1)
            // overload should win, firing exactly 1 batched CollectionChanged event.
            var collection = new ObservableCollection<int> { 1 };
            var events = new List<NotifyCollectionChangedEventArgs>();
            collection.CollectionChanged += (s, e) => events.Add(e);

            collection.AddRange(new[] { 2, 3, 4 });

            // If the Collection<T> version ran instead, we'd see 3 individual Add events.
            events.Should().HaveCount(1);
            events[0].Action.Should().Be(NotifyCollectionChangedAction.Add);
            events[0].NewItems.Cast<int>().Should().HaveCount(3);
        }

        #endregion

        #region Reentrancy Tests

        [TestMethod]
        public void AddRange_ReentrantModification_WithMultipleHandlers_ThrowsInvalidOperationException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };
            // Two handlers required — single-handler reentrancy is permitted by ObservableCollection.
            collection.CollectionChanged += (s, e) => { };
            collection.CollectionChanged += (s, e) =>
            {
                if (e.Action is NotifyCollectionChangedAction.Add)
                {
                    collection.AddRange(new[] { 99 });
                }
            };

            Action act = () => collection.AddRange(new[] { 4, 5 });

            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void InsertRange_ReentrantModification_WithMultipleHandlers_ThrowsInvalidOperationException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };
            collection.CollectionChanged += (s, e) => { };
            collection.CollectionChanged += (s, e) =>
            {
                if (e.Action is NotifyCollectionChangedAction.Add)
                {
                    collection.InsertRange(0, new[] { 99 });
                }
            };

            Action act = () => collection.InsertRange(0, new[] { 4, 5 });

            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void RemoveRange_ReentrantModification_WithMultipleHandlers_ThrowsInvalidOperationException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            collection.CollectionChanged += (s, e) => { };
            collection.CollectionChanged += (s, e) =>
            {
                if (e.Action is NotifyCollectionChangedAction.Remove)
                {
                    collection.RemoveRange(0, 1);
                }
            };

            Action act = () => collection.RemoveRange(0, 2);

            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void ReplaceRange_ReentrantModification_WithMultipleHandlers_ThrowsInvalidOperationException()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            collection.CollectionChanged += (s, e) => { };
            collection.CollectionChanged += (s, e) =>
            {
                if (e.Action is NotifyCollectionChangedAction.Replace)
                {
                    collection.ReplaceRange(0, 1, new[] { 99 });
                }
            };

            Action act = () => collection.ReplaceRange(0, 2, new[] { 10, 20 });

            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void AddRange_ReentrantModification_WithSingleHandler_Succeeds()
        {
            var collection = new ObservableCollection<int> { 1, 2, 3 };
            var reentrantCallMade = false;

            // Single handler — reentrancy IS allowed by ObservableCollection's design.
            collection.CollectionChanged += (s, e) =>
            {
                if (e.Action is NotifyCollectionChangedAction.Add && !reentrantCallMade)
                {
                    reentrantCallMade = true;
                    collection.AddRange(new[] { 99 });
                }
            };

            collection.AddRange(new[] { 4, 5 });

            reentrantCallMade.Should().BeTrue();
            collection.Should().Contain(99);
        }

        #endregion

    }

}
