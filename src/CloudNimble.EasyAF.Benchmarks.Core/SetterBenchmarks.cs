using BenchmarkDotNet.Attributes;
using EasyAFModel;
using System;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.Benchmarks
{

    /// <summary>
    /// Compares the generated <see cref="Product"/> (<c>Set(nameof(Id), ref _id, value)</c>) with the EasyAF 4.x generated
    /// <see cref="V4.Product"/> (<c>Set(() =&gt; Id, ref _id, value)</c>), and measures what constructing an entity costs.
    /// </summary>
    [MemoryDiagnoser]
    public class SetterBenchmarks
    {

        #region Fields

        private static readonly Guid GuidA = Guid.NewGuid();
        private static readonly Guid GuidB = Guid.NewGuid();
        private static readonly DateTimeOffset DateA = DateTimeOffset.UtcNow;
        private static readonly DateTimeOffset DateB = DateA.AddMinutes(1);

        private Product _v5;
        private V4.Product _v4;
        private bool _flip;

        #endregion

        #region Properties

        /// <summary>
        /// Whether something (for example UI binding) is subscribed to PropertyChanged, which makes every change allocate event args.
        /// </summary>
        [Params(false, true)]
        public bool HasSubscriber { get; set; }

        #endregion

        #region Setup

        /// <summary>
        /// Creates both entities and optionally subscribes to their PropertyChanged events.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _v5 = new Product();
            _v4 = new V4.Product();
            if (HasSubscriber)
            {
                _v5.PropertyChanged += (_, _) => { };
                _v4.PropertyChanged += (_, _) => { };
            }
        }

        #endregion

        #region Benchmarks

        /// <summary>
        /// Reproduces the EasyAF 4.x constructor, which also created an empty OriginalValues dictionary for every entity.
        /// </summary>
        /// <remarks>
        /// <see cref="V4.Product"/> inherits the EasyAF 5.0 DbObservableObject, so the dictionary the 4.x base class created is added here.
        /// </remarks>
        [Benchmark]
        public (V4.Product, Dictionary<string, object>) Construct_V4() => (new(), new());

        /// <summary>
        /// Constructs a generated entity, the way EF does for every materialized row.
        /// </summary>
        [Benchmark]
        public Product Construct_V5() => new();

        /// <summary>
        /// Sets seven properties with the EasyAF 4.x expression-tree setters. Values alternate so every assignment is a real change.
        /// </summary>
        [Benchmark(Baseline = true)]
        public void SetSevenProperties_V4()
        {
            _flip = !_flip;
            var p = _v4;
            p.Id = _flip ? GuidA : GuidB;
            p.DisplayName = _flip ? "A" : "B";
            p.StatusTypeId = _flip ? GuidA : GuidB;
            p.CreatedById = _flip ? GuidA : GuidB;
            p.DateCreated = _flip ? DateA : DateB;
            p.UpdatedById = _flip ? GuidA : GuidB;
            p.DateUpdated = _flip ? DateA : DateB;
        }

        /// <summary>
        /// Sets the same seven properties with the generated <c>nameof</c> setters.
        /// </summary>
        [Benchmark]
        public void SetSevenProperties_V5()
        {
            _flip = !_flip;
            var p = _v5;
            p.Id = _flip ? GuidA : GuidB;
            p.DisplayName = _flip ? "A" : "B";
            p.StatusTypeId = _flip ? GuidA : GuidB;
            p.CreatedById = _flip ? GuidA : GuidB;
            p.DateCreated = _flip ? DateA : DateB;
            p.UpdatedById = _flip ? GuidA : GuidB;
            p.DateUpdated = _flip ? DateA : DateB;
        }

        #endregion

    }

}
