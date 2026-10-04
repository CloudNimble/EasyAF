using Ben.Collections;
using BenchmarkDotNet.Attributes;
using CloudNimble.EasyAF.Core;
using EasyAFModel;
using System;
using System.Linq;

namespace CloudNimble.EasyAF.Benchmarks
{

    /// <summary>
    /// Compares how EntityManager detects the four audit interfaces on an entity: the EasyAF 4.x type dictionary and LINQ lookup,
    /// and the EasyAF 5.0 pattern matching.
    /// </summary>
    /// <remarks>
    /// Only the detection is measured, not the assignments, so the ID claim lookup doesn't hide the difference.
    /// </remarks>
    [MemoryDiagnoser]
    public class AuditInterfaceBenchmarks
    {

        #region Fields

        private static readonly TypeDictionary<Type[]> InterfaceDictionary = new();

        private DbObservableObject _entity;
        private DbObservableObject _v4Entity;

        #endregion

        #region Setup

        /// <summary>
        /// Creates the entity and warms the 4.x cache, the way it would be after the first save.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _entity = new Product();
            _v4Entity = new V4.Product();
            InterfaceDictionary[typeof(V4.Product)] = typeof(V4.Product).GetInterfaces();
        }

        #endregion

        #region Benchmarks

        /// <summary>
        /// The EasyAF 4.x EntityManager.ResetAuditProperties detection.
        /// </summary>
        [Benchmark(Baseline = true)]
        public int DetectAuditInterfaces_V4()
        {
            var entity = _v4Entity;
            var entityType = entity.GetType();
            if (!InterfaceDictionary.ContainsKey(entityType))
            {
                InterfaceDictionary[entityType] = entityType.GetInterfaces();
            }

            var found = 0;
            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(ICreatorTrackable<>).Name) && entity as ICreatorTrackable<Guid> is not null) found++;
            if (InterfaceDictionary[entityType].Any(c => c == typeof(ICreatedAuditable)) && entity as ICreatedAuditable is not null) found++;
            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(IUpdaterTrackable<>).Name) && entity as IUpdaterTrackable<Guid> is not null) found++;
            if (InterfaceDictionary[entityType].Any(c => c == typeof(IUpdatedAuditable)) && entity as IUpdatedAuditable is not null) found++;
            return found;
        }

        /// <summary>
        /// The EasyAF 5.0 EntityManager.ResetAuditProperties detection.
        /// </summary>
        [Benchmark]
        public int DetectAuditInterfaces_V5()
        {
            var entity = _entity;
            var found = 0;
            if (entity is ICreatorTrackable<Guid>) found++;
            if (entity is ICreatedAuditable) found++;
            if (entity is IUpdaterTrackable<Guid>) found++;
            if (entity is IUpdatedAuditable) found++;
            return found;
        }

        #endregion

    }

}
