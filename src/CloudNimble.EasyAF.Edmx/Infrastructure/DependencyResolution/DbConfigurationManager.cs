// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    // <summary>
    // This class is responsible for managing the app-domain instance of the <see cref="DbConfiguration" /> class.
    // This includes loading from config, discovery from the context assembly and pushing/popping configurations
    // used by <see cref="DbContextInfo" />.
    // </summary>
    internal class DbConfigurationManager
    {
        private static readonly DbConfigurationManager _configManager
            = new(new DbConfigurationLoader(), new DbConfigurationFinder());

        private EventHandler<DbConfigurationLoadedEventArgs> _loadedHandler;

        private readonly DbConfigurationLoader _loader;
        private readonly DbConfigurationFinder _finder;

        private readonly Lazy<InternalConfiguration> _configuration;
        private volatile DbConfiguration _newConfiguration;
        private volatile Type _newConfigurationType = typeof(DbConfiguration);

        private readonly object _lock = new();

        // We don't need a dictionary here, just a set, but there is no ConcurrentSet in the BCL.
        private readonly ConcurrentDictionary<Assembly, object> _knownAssemblies = new();

        private readonly Lazy<List<Tuple<AppConfig, InternalConfiguration>>> _configurationOverrides
            = new(
                () => []);

        public DbConfigurationManager(DbConfigurationLoader loader, DbConfigurationFinder finder)
        {
            DebugCheck.NotNull(loader);
            DebugCheck.NotNull(finder);
            
            _loader = loader;
            _finder = finder;
            _configuration = new Lazy<InternalConfiguration>(
                () =>
                    {
                        var configuration = _newConfiguration
                                            ?? _newConfigurationType.CreateInstance<DbConfiguration>(
                                                Strings.CreateInstance_BadDbConfigurationType);

                        configuration.InternalConfiguration.Lock();
                        return configuration.InternalConfiguration;
                    });
        }

        public static DbConfigurationManager Instance
        {
            get { return _configManager; }
        }

        public virtual void AddLoadedHandler(EventHandler<DbConfigurationLoadedEventArgs> handler)
        {
            DebugCheck.NotNull(handler);

            if (ConfigurationSet)
            {
                throw new InvalidOperationException(Strings.AddHandlerToInUseConfiguration);
            }
            _loadedHandler += handler;
        }

        public virtual void RemoveLoadedHandler(EventHandler<DbConfigurationLoadedEventArgs> handler)
        {
            DebugCheck.NotNull(handler);

            _loadedHandler -= handler;
        }

        public virtual void OnLoaded(InternalConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            var eventArgs = new DbConfigurationLoadedEventArgs(configuration);

            var handler = _loadedHandler;
            if (handler is not null)
            {
                handler(configuration.Owner, eventArgs);
            }

            configuration.DispatchLoadedInterceptors(eventArgs);
        }

        public virtual InternalConfiguration GetConfiguration()
        {
            // The common case is that no overrides have ever been set so we don't take the time to do
            // the locking and checking.
            if (_configurationOverrides.IsValueCreated)
            {
                lock (_lock)
                {
                    if (_configurationOverrides.Value.Count != 0)
                    {
                        return _configurationOverrides.Value.Last().Item2;
                    }
                }
            }

            return _configuration.Value;
        }

        public virtual void SetConfigurationType(Type configurationType)
        {
            DebugCheck.NotNull(configurationType);

            _newConfigurationType = configurationType;
        }

        public virtual void SetConfiguration(InternalConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            var configurationType = _loader.TryLoadFromConfig(AppConfig.DefaultInstance);
            if (configurationType is not null)
            {
                configuration = configurationType
                    .CreateInstance<DbConfiguration>(Strings.CreateInstance_BadDbConfigurationType)
                    .InternalConfiguration;
            }

            _newConfiguration = configuration.Owner;

            if (_configuration.Value.Owner.GetType() != configuration.Owner.GetType())
            {
                if (_configuration.Value.Owner.GetType() == typeof(DbConfiguration))
                {
                    throw new InvalidOperationException(Strings.DefaultConfigurationUsedBeforeSet(configuration.Owner.GetType().Name));
                }

                throw new InvalidOperationException(
                    Strings.ConfigurationSetTwice(configuration.Owner.GetType().Name, _configuration.Value.Owner.GetType().Name));
            }
        }

        public virtual void EnsureLoadedForContext(Type contextType)
        {
            DebugCheck.NotNull(contextType);
            Debug.Assert(typeof(DbContext).IsAssignableFrom(contextType));

            EnsureLoadedForAssembly(contextType.Assembly(), contextType);
        }

        public virtual void EnsureLoadedForAssembly(Assembly assemblyHint, Type contextTypeHint)
        {
            DebugCheck.NotNull(assemblyHint);

            if (contextTypeHint == typeof(DbContext)
                || _knownAssemblies.ContainsKey(assemblyHint))
            {
                return;
            }

            if (_configurationOverrides.IsValueCreated)
            {
                lock (_lock)
                {
                    if (_configurationOverrides.Value.Count != 0)
                    {
                        return;
                    }
                }
            }

            if (!ConfigurationSet)
            {
                var foundConfigurationType =
                    _loader.TryLoadFromConfig(AppConfig.DefaultInstance) ??
                    _finder.TryFindConfigurationType(assemblyHint, _finder.TryFindContextType(assemblyHint, contextTypeHint));

                if (foundConfigurationType is not null)
                {
                    SetConfigurationType(foundConfigurationType);
                }
            }
            else if (!assemblyHint.IsDynamic // Don't throw for proxy contexts created in dynamic assemblies
                     && !_loader.AppConfigContainsDbConfigurationType(AppConfig.DefaultInstance))
            {
                contextTypeHint = _finder.TryFindContextType(assemblyHint, contextTypeHint);
                var foundType = _finder.TryFindConfigurationType(assemblyHint, contextTypeHint);
                if (foundType is not null)
                {
                    if (_configuration.Value.Owner.GetType() == typeof(DbConfiguration))
                    {
                        throw new InvalidOperationException(Strings.ConfigurationNotDiscovered(foundType.Name));
                    }
                    if (contextTypeHint is not null && foundType != _configuration.Value.Owner.GetType())
                    {
                        throw new InvalidOperationException(
                            Strings.SetConfigurationNotDiscovered(_configuration.Value.Owner.GetType().Name, contextTypeHint.Name));
                    }
                }
            }

            _knownAssemblies.TryAdd(assemblyHint, null);
        }

        private bool ConfigurationSet
        {
            get { return _configuration.IsValueCreated; }
        }

        public virtual bool PushConfiguration(AppConfig config, Type contextType)
        {
            DebugCheck.NotNull(config);
            DebugCheck.NotNull(contextType);
            Debug.Assert(typeof(DbContext).IsAssignableFrom(contextType));

            // Perf optimization: if there is no change to the default app-domain config and if the
            // context assembly has already been checked for configurations, then avoid creating
            // and pushing a new configuration since it would be the same as the current one anyway.
            if (config == AppConfig.DefaultInstance
                && (contextType == typeof(DbContext) || _knownAssemblies.ContainsKey(contextType.Assembly())))
            {
                return false;
            }

            var configuration = (_loader.TryLoadFromConfig(config)
                                 ?? _finder.TryFindConfigurationType(contextType)
                                 ?? typeof(DbConfiguration))
                .CreateInstance<DbConfiguration>(Strings.CreateInstance_BadDbConfigurationType)
                .InternalConfiguration;

            configuration.SwitchInRootResolver(_configuration.Value.RootResolver);
            configuration.AddAppConfigResolver(new AppConfigDependencyResolver(config, configuration));

            lock (_lock)
            {
                _configurationOverrides.Value.Add(Tuple.Create(config, configuration));
            }

            configuration.Lock();

            return true;
        }

        public virtual void PopConfiguration(AppConfig config)
        {
            DebugCheck.NotNull(config);

            lock (_lock)
            {
                var configuration = _configurationOverrides.Value.FirstOrDefault(c => c.Item1 == config);
                if (configuration is not null)
                {
                    _configurationOverrides.Value.Remove(configuration);
                }
            }
        }
    }
}
