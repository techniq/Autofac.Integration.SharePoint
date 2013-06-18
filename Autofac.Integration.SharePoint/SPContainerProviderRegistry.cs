using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;
using Microsoft.SharePoint;
using System.Diagnostics;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint
{
    internal class SPContainerProviderRegistry : IDisposable
    {
        private bool _disposed = false;
        private readonly Dictionary<Guid, SPContainerProviderInfo> _containerProviders = new Dictionary<Guid, SPContainerProviderInfo>();
        private int _refreshPeriod = 30;

        public int RefreshPeriod
        {
            get { return _refreshPeriod; }
            set { _refreshPeriod = value; }
        }

        /// <summary>
        /// Gets all existing container providers.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<SPContainerProvider> GetContainerProviders()
        {
            lock(_containerProviders)
            {
                return _containerProviders.Values.Select(cpi => cpi.ContainerProvider).ToList();
            }
        }

        internal SPContainerProvider GetOrCreateContainerProvider(SPSite site)
        {
            var sharePointScope = new SiteBoundary(site);
            return GetOrCreateContainerProvider(sharePointScope);
        }

        internal SPContainerProvider GetOrCreateContainerProvider(SPWebApplication webApp)
        {
            var sharePointScope = new WebApplicationBoundary(webApp);
            return GetOrCreateContainerProvider(sharePointScope);
        }

        internal SPContainerProvider GetOrCreateContainerProvider()
        {
            var sharePointScope = new FarmBoundary();
            return GetOrCreateContainerProvider(sharePointScope);
        }

        internal bool TryGetContainerProvider(Guid id, out SPContainerProviderInfo containerProviderInfo)
        {
            lock (_containerProviders)
            {
                if (_containerProviders.TryGetValue(id, out containerProviderInfo))
                {
                    return true;
                }
            }
            containerProviderInfo = null;
            return false;
        }

        private SPContainerProvider GetOrCreateContainerProvider(ISharePointBoundary tmp)
        {
            SPContainerProviderInfo cpInf;
            if (TryGetContainerProvider(tmp.ID, out cpInf))
            {
                var diff = DateTime.Now - cpInf.LastRefresh;
                if (diff.TotalSeconds >= RefreshPeriod)
                {
                    Trace.WriteLine(string.Format(Messages.ContainerProviderNotValidAnymore, tmp, diff.TotalSeconds));
                    return UpdateContainerProviderIfNecessary(tmp, cpInf);
                }
                // no updates required --> just return provider
                return cpInf.ContainerProvider;
            }
            // no provider exists, return one
            return CreateContainerProvider(tmp);
        }

        private SPContainerProvider CreateContainerProvider(ISharePointBoundary sharePointScope)
        {
            IList<IModule> modules = Configuration.GetConfiguredModules(sharePointScope.PropertyProvider);
            return CreateContainerProvider(sharePointScope, modules);
        }

        private SPContainerProvider CreateContainerProvider(ISharePointBoundary sharePointScope, IEnumerable<IModule> modules)
        {
            var builder = new SPContainerBuilder();
            foreach (IModule module in modules)
            {
                Trace.WriteLine(string.Format(Messages.RegisteringModule, module.GetType().AssemblyQualifiedName, sharePointScope));
                builder.RegisterModule(module);
            }

            SPContainerProvider containerProvider = new SPContainerProvider(builder.Build());
            lock(_containerProviders)
            {
                _containerProviders[sharePointScope.ID] = new SPContainerProviderInfo(containerProvider);
            }

            return containerProvider;
        }

        private SPContainerProvider UpdateContainerProviderIfNecessary(ISharePointBoundary sharePointScope, SPContainerProviderInfo providerInfo)
        {
            var propertyProvider = sharePointScope.PropertyProvider;
            var configModules = Configuration.ReadAllModuleConfigurations(propertyProvider);
            foreach(var configModule in configModules)
            {
                // if the configuration is newer than the current provider
                // which means that new modules were added, since the provider was built
                if (configModule.ModifiedAt > providerInfo.LastRefresh)
                {
                    Trace.WriteLine(string.Format(Messages.ContainerProviderNeedsUpdate, sharePointScope, providerInfo.LastRefresh, configModule.ModifiedAt, Enum.GetName(typeof(SPScope), configModule.Scope)));
                    return CreateContainerProvider(sharePointScope, Configuration.ReadAllModules(configModules));
                }
            }

            Trace.WriteLine(string.Format(Messages.ContainerProviderNeedsNoUpdate, sharePointScope));
            // nothing changed, return "old" provider which is still valid);)
            return providerInfo.ContainerProvider;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            lock (_containerProviders)
            {
                // SPWebApplication and SPSite
                var allProviders = _containerProviders.Values.Where(cpi => cpi.ContainerProvider != null && cpi.ContainerProvider.ApplicationContainer != null)
                                                            .Select(cpi => cpi.ContainerProvider)
                                                            .ToList();

                foreach (var cp in allProviders)
                    cp.ApplicationContainer.Dispose();

                // clear all
                _containerProviders.Clear();
            }
        }
    }

    internal class SPContainerProviderInfo
    {
        private SPContainerProvider _containerProvider;

        internal SPContainerProviderInfo(SPContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            LastRefresh = DateTime.Now;
        }

        internal SPContainerProvider ContainerProvider
        {
            get { return _containerProvider; }
            set
            {
                _containerProvider = value;
                LastRefresh = DateTime.Now;
            }
        }

        internal DateTime LastRefresh { get; private set; }
    }
}
