using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Integration.SharePoint.Integration;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint
{
    public class PropertyProviderFactory
    {
        public static IPropertyProvider GetPropertyProvider(SPSite site)
        {
            if(site == null)
                throw new ArgumentNullException("site");
            return new SitePropertyProvider(site);
        }

        public static IPropertyProvider GetPropertyProvider(SPWebApplication webApplication)
        {
            if (webApplication == null)
                throw new ArgumentNullException("webApplication");
            return new WebApplicationPropertyProvider(webApplication);
        }

        public static IPropertyProvider GetPropertyProvider()
        {
            return new FarmPropertyProvider();
        }
    }

    public interface IIndexAccessible
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key at the direct scope of the provider.
        /// </summary>
        object this[object key] { get; set; }
        /// <summary>
        /// Gets the scope.
        /// </summary>
        SPScope Scope { get; }
        /// <summary>
        /// Gets the ID.
        /// </summary>
        Guid ID { get; }
    }

    public interface IPropertyProvider : IIndexAccessible
    {
        /// <summary>
        /// Gets all properties recursively from the providers direct scope (e.g.: Site) and all parent scopes (WebApplication, Farm).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>a list of found propertyentries</returns>
        IEnumerable<PropertyEntry> GetAllPropertiesRecursive(string key);
        /// <summary>
        /// Gets all properties recursive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extractorFunction">The extractor function.</param>
        /// <returns></returns>
        IEnumerable<T> GetAllPropertiesRecursive<T>(Func<IIndexAccessible, T> extractorFunction);

        void Update();
    }

    public class PropertyEntry
    {
        public PropertyEntry(object value, SPScope scope)
        {
            this.Value = value;
            this.Scope = scope;
        }

        public object Value { get; private set; }
        public SPScope Scope { get; private set; }
    }

    public class SitePropertyProvider : IPropertyProvider
    {
        private SPSite _site;
        public SitePropertyProvider(SPSite site)
        {
            if(site == null)
                throw new ArgumentNullException("site");
            _site = site;
        }

        public IEnumerable<PropertyEntry> GetAllPropertiesRecursive(string key)
        {
            List<PropertyEntry> results = new List<PropertyEntry>();
            if(_site != null && _site.RootWeb != null && _site.RootWeb.AllProperties.ContainsKey(key))
                results.Add(new PropertyEntry(_site.RootWeb.AllProperties[key], SPScope.SPSite));
            if(_site != null && _site.WebApplication != null)
                PropertyHelper.AddWebApplicationProperties(key, _site.WebApplication, results);

            PropertyHelper.AddFarmProperties(key, results);

            return results;
        }

        public IEnumerable<T> GetAllPropertiesRecursive<T>(Func<IIndexAccessible, T> extractorFunction)
        {
            List<T> results = new List<T>();
            results.Add(extractorFunction(this));
            results.Add(extractorFunction(new WebApplicationPropertyProvider(_site.WebApplication)));
            results.Add(extractorFunction(new FarmPropertyProvider()));
            return results;
        }

        public void Update()
        {
            _site.RootWeb.Update();
        }

        public object this[object key]
        {
            get
            {
                if (_site.RootWeb != null)
                    return _site.RootWeb.AllProperties[key];
                return null;
            }
            set
            {
                if (_site.RootWeb != null)
                    _site.RootWeb.AllProperties[key] = value;
                else
                    throw new InvalidOperationException(string.Format(ContainerDisposalModuleResources.RootWebNull, _site.Url));
            }
        }

        public SPScope Scope
        {
            get { return SPScope.SPSite; }
        }

        public Guid ID
        {
            get { return _site.ID; }
        }
    }

    public class WebApplicationPropertyProvider : IPropertyProvider
    {
        private SPWebApplication _webApplication;
      
        public WebApplicationPropertyProvider(SPWebApplication webApplication)
        {
            if(webApplication == null)
                throw new ArgumentNullException("webApplication");
            _webApplication = webApplication;
        }

        public IEnumerable<PropertyEntry> GetAllPropertiesRecursive(string key)
        {
            List<PropertyEntry> results = new List<PropertyEntry>();
            
            if (_webApplication != null)
                PropertyHelper.AddWebApplicationProperties(key, _webApplication, results);

            PropertyHelper.AddFarmProperties(key, results);

            return results;
        }

        public IEnumerable<T> GetAllPropertiesRecursive<T>(Func<IIndexAccessible, T> extractorFunction)
        {
            List<T> results = new List<T>();
            results.Add(extractorFunction(this));
            results.Add(extractorFunction(new FarmPropertyProvider()));
            return results;
        }

        public void Update()
        {
            _webApplication.Update();
        }

        public object this[object key]
        {
            get
            {
                return _webApplication.Properties[key];
            }
            set
            {
                _webApplication.Properties[key] = value;
            }
        }

        public SPScope Scope
        {
            get { return SPScope.SPWebApplication; }
        }

        public Guid ID
        {
            get { return _webApplication.Id; }
        }
    }

    public class FarmPropertyProvider : IPropertyProvider
    {
        private SPFarm _farm;

        public  FarmPropertyProvider()
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
                                                     {
                                                         _farm = SPFarm.Local;
                                                     });
        }

        public IEnumerable<PropertyEntry> GetAllPropertiesRecursive(string key)
        {
            List<PropertyEntry> results = new List<PropertyEntry>();

            PropertyHelper.AddFarmProperties(key, results);

            return results;
        }

        public IEnumerable<T> GetAllPropertiesRecursive<T>(Func<IIndexAccessible, T> extractorFunction)
        {
            List<T> results = new List<T>();
            results.Add(extractorFunction(this));
            return results;
        }

        public void Update()
        {
            _farm.Update(true);
        }

        public object this[object key]
        {
            get
            {
                object result = null;
                     result = _farm.Properties[key];                     
                return result;
            }
            set
            {
                _farm.Properties[key] = value;
            }
        }

        public SPScope Scope
        {
            get { return SPScope.SPFarm; }
        }

        public Guid ID
        {
            get { return Guid.Empty; }
        }
    }

    internal static class PropertyHelper
    {
        internal static void AddWebApplicationProperties(string key, SPWebApplication webApplication, List<PropertyEntry> results)
        {
            if (webApplication != null && webApplication.Properties.ContainsKey(key))
                results.Add(new PropertyEntry(webApplication.Properties[key], SPScope.SPWebApplication));
        }

        internal static void AddFarmProperties(string key, List<PropertyEntry> results)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                var farm = SPFarm.Local;
                if (farm != null && farm.Properties.ContainsKey(key))
                    results.Add(new PropertyEntry(farm.Properties[key], SPScope.SPFarm));
            });
        }
    }
}
