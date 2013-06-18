using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint
{
    public class SPServiceLocator
    {
        private static readonly SPContainerProviderRegistry _registry = new SPContainerProviderRegistry();
        private static bool _initialized;

        /// <summary>
        /// Gets or sets the refresh period in seconds.
        /// </summary>
        /// <value>
        /// The refresh period in seconds.
        /// </value>
        public static int RefreshPeriod
        {
            get { return _registry.RefreshPeriod; }
            set { _registry.RefreshPeriod = value; }
        }

        internal static SPContainerProvider GetContainerProvider(SPSite site)
        {
            if(site == null)
                throw new ArgumentNullException("site");
            return _registry.GetOrCreateContainerProvider(site);
        }

        /// <summary>
        /// Gets all existing container providers.
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<SPContainerProvider> GetContainerProviders()
        {
            return _registry.GetContainerProviders();
        }

        public static bool RequestLifetimeIsUnavailable
        {
            get
            {
                SPContainerProviderInfo cpInf;
                if(_registry.TryGetContainerProvider(Guid.Empty, out cpInf))
                    return cpInf.ContainerProvider.RequestLifetimeIsUnavailable;
                return true;
            }
        }

        //internal static void InitializeContainer()
        //{
        //    InitializeContainer((string) null);
        //}

        ///// <summary>
        ///// This method is available for testing, or to use the ServiceLocator capability outside of
        ///// the SharePoint HttpContext (i.e.: console app, linqpad, powershell, etc...)
        ///// <remarks>Client MUST be on the SharePoint server</remarks>
        ///// </summary>
        ///// <param name="site">site that provides context information</param>
        //public static void InitializeContainer(SPSite site)
        //{
        //    InitializeContainer(site, null);
        //}

        //public static void InitializeContainer(SPSite site, Action<ContainerBuilder> additionalBuild)
        //{
        //    string configFile = null;

        //    if(site != null)
        //        configFile = Path.Combine(site.WebApplication.GetIisSettingsWithFallback(SPUrlZone.Default).Path.ToString(), "web.config");
            
        //    InitializeContainer(configFile, site, additionalBuild);
        //}

        //public static void InitializeContainer(string configFile, SPSite site)
        //{
        //    InitializeContainer(configFile, site, null);
        //}

        //public static void InitializeContainer(string configFile)
        //{
        //    InitializeContainer(configFile, null, null);
        //}

        ///// <summary>
        ///// This method is available for testing, or to use the ServiceLocator capability outside of
        ///// the SharePoint HttpContext (i.e.: console app, linqpad, powershell, etc...)
        ///// </summary>
        ///// <param name="configFile">Autofac configuration file</param>
        ///// <param name="site">the web provided as context for the service_locator</param>
        ///// <param name="additionalBuild">Any additional build information desired (will override the web.config and farm properties)</param>
        //public static void InitializeContainer(string configFile, SPSite site, Action<ContainerBuilder> additionalBuild)
        //{
        //    if (_initialized)
        //        return;

        //    if (!string.IsNullOrEmpty(configFile) && !File.Exists(configFile))
        //        throw new FileNotFoundException("The autofac configuration file could not be found.", configFile);

        //    var builder = new SPContainerBuilder();

        //    // register modules from config file
        //    builder.RegisterModule(string.IsNullOrEmpty(configFile)
        //                               ? new ConfigurationSettingsReader("autofac")
        //                               : new ConfigurationSettingsReader("autofac", configFile));

        //    // register modules from SPSite, SPWebApplication and SPFarm.Properties
        //    //IList<IModule> modules = SPContainerBuilder.GetSitePropertyConfiguredModules(site);
        //    IList<IModule> modules = Configuration.GetFarmPropertyConfiguredModules();
        //    foreach (IModule module in modules)
        //        builder.RegisterModule(module);

        //    if (additionalBuild != null)
        //        additionalBuild.Invoke(builder);

        //    _containerProvider = new SPContainerProvider(builder.Build());
        //    _initialized = true;
        //}

        public static ILifetimeScope NewDisposableLifetime(SPSite site)
        {
            return NewDisposableLifetime(site, null);
        }

        public static ILifetimeScope NewDisposableLifetime(SPSite site, string name)
        {
            if (site == null)
                throw new ArgumentNullException("site");
            return NewDisposableLifetime(_registry.GetOrCreateContainerProvider(site), name);
        }

        public static ILifetimeScope NewDisposableLifetime(SPWebApplication webApplication)
        {
            return NewDisposableLifetime(webApplication, null);
        }

        public static ILifetimeScope NewDisposableLifetime(SPWebApplication webApplication, string name)
        {
            if (webApplication == null)
                throw new ArgumentNullException("webApplication");
            return NewDisposableLifetime(_registry.GetOrCreateContainerProvider(webApplication), name);
        }

        public static ILifetimeScope NewDisposableLifetime()
        {
            return NewDisposableLifetime(_registry.GetOrCreateContainerProvider(), null);
        }

        public static ILifetimeScope NewDisposableLifetime(string name)
        {
            return NewDisposableLifetime(_registry.GetOrCreateContainerProvider(), name);
        }

        private static ILifetimeScope NewDisposableLifetime(SPContainerProvider containerProvider, string name)
        {
            return string.IsNullOrEmpty(name)
                        ? containerProvider.NewDisposableLifetime()
                        : containerProvider.NewDisposableLifetime(name);
        }

        public static ILifetimeScope GetRequestLifetime()
        {
            return _registry.GetOrCreateContainerProvider().GetRequestLifetime();
        }

        public static ILifetimeScope GetRequestLifetime(SPSite site)
        {
            if(site == null)
                throw new ArgumentNullException("site");
            return _registry.GetOrCreateContainerProvider(site).GetRequestLifetime();
        }

        public static ILifetimeScope GetRequestLifetime(SPWebApplication webApplication)
        {
            if (webApplication == null)
                throw new ArgumentNullException("webApplication");
            return _registry.GetOrCreateContainerProvider(webApplication).GetRequestLifetime();
        }

        public static void Kill()
        {
            _registry.Dispose();
            _initialized = false;
        }
    }
}