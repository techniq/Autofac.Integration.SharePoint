using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Container = Autofac.Core.Container;
using System.Diagnostics;

namespace Autofac.Integration.SharePoint
{
    public class SPContainerBuilder : ContainerBuilder
    {
        /// <summary>
        /// Registers the module at farm scope. (farm properties)
        /// Intended only for SPFarm feature receiver
        /// </summary>
        /// <typeparam name="T">the type of the module</typeparam>
        /// <param name="properties">The properties.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="replaceIfExists">if set to <c>true</c> [replace if exists].</param>
        public static void RegisterModule<T>(IDictionary<string, string> properties = null,
                                             IDictionary<string, string> parameters = null, bool replaceIfExists = true)
            where T : IModule
        {
            IPropertyProvider propertyProvider = new FarmPropertyProvider();
            RegisterModule<T>(parameters, properties, propertyProvider, replaceIfExists);
        }

        /// <summary>
        /// Registers the module at web application scope. (web application properties)
        /// Intended only for SPWebApplication feature receiver
        /// </summary>
        /// <typeparam name="T">the type of the module</typeparam>
        /// <param name="webApplication">The web application.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="replaceIfExists">if set to <c>true</c> [replace if exists].</param>
        public static void RegisterModule<T>(SPWebApplication webApplication, IDictionary<string, string> properties = null,
                                             IDictionary<string, string> parameters = null, bool replaceIfExists = true)
            where T : IModule

        {
            IPropertyProvider propertyProvider = new WebApplicationPropertyProvider(webApplication);
            RegisterModule<T>(parameters, properties, propertyProvider, replaceIfExists);
        }

        /// <summary>
        /// Registers the module at site scope. (site rootweb properties)
        /// Intended only for SPSite feature receiver
        /// </summary>
        /// <typeparam name="T">the type of the module</typeparam>
        /// <param name="site">The site.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="replaceIfExists">if set to <c>true</c> [replace if exists].</param>
        public static void RegisterModule<T>(SPSite site, IDictionary<string, string> properties = null,
                                             IDictionary<string, string> parameters = null, bool replaceIfExists = true)
            where T : IModule
        {
            IPropertyProvider propertyProvider = new SitePropertyProvider(site);
            RegisterModule<T>(parameters, properties, propertyProvider, replaceIfExists);
        }
        
        private static void RegisterModule<T>(IDictionary<string, string> parameters, IDictionary<string, string> properties, IPropertyProvider propertyProvider, bool replaceIfExists)
        {
            string assenblyQualifiedName = typeof(T).AssemblyQualifiedName;
            // create the module
            var moduleConfigEntry = new XElement(XName.Get("module", ""));
            moduleConfigEntry.SetAttributeValue(XName.Get("type", ""), assenblyQualifiedName);
            if (parameters != null && parameters.Any())
            {
                var xepc = new XElement(XName.Get("parameters", ""));
                foreach (string parameter in parameters.Keys)
                {
                    var xep = new XElement(XName.Get("parameter", ""));
                    xep.SetAttributeValue(XName.Get("name", ""), parameter);
                    xep.SetAttributeValue(XName.Get("value", ""), parameters[parameter]);
                    xepc.Add(xep);
                }
                moduleConfigEntry.Add(xepc);
            }
            if (properties != null && properties.Any())
            {
                var xepc = new XElement(XName.Get("properties", ""));
                foreach (string property in properties.Keys)
                {
                    var xep = new XElement(XName.Get("property", ""));
                    xep.SetAttributeValue(XName.Get("name", ""), property);
                    xep.SetAttributeValue(XName.Get("value", ""), properties[property]);
                    xepc.Add(xep);
                }
                moduleConfigEntry.Add(xepc);
            }

            ModuleConfiguration modConfig = Configuration.ReadModuleConfiguration(propertyProvider);

            if (modConfig == null)
                modConfig = new ModuleConfiguration("<modules/>", propertyProvider.Scope, DateTime.Now,
                                                    propertyProvider.ID);

            XElement xm = modConfig.Configuration;
            bool exists = xm.XPathSelectElements("module[@type='" + assenblyQualifiedName + "']").Any();
            if (exists && !replaceIfExists)
            {
                Trace.WriteLine(string.Format("module {0} already defined at {1} and replaceIfExists set to {2} --> skipping replacement", assenblyQualifiedName, propertyProvider.ToString(), replaceIfExists));
                return;
            }
            if (exists)
            {
                RemoveModule<T>(propertyProvider, false);
                // refresh xm
                xm = Configuration.ReadModuleConfiguration(propertyProvider).Configuration;
            }
            xm.Add(moduleConfigEntry);
            Configuration.UpdatePersistedModules(xm, propertyProvider);
            
        }

        /// <summary>
        /// Removes the module from the farm properties.
        /// Intended only for SPFarm feature receiver
        /// </summary>
        /// <typeparam name="T">type of the module</typeparam>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        public static void RemoveModule<T>(bool recursive = false)
            where T : IModule
        {
            IPropertyProvider propertyProvider = new FarmPropertyProvider();
            RemoveModule<T>(propertyProvider, recursive);
        }

        /// <summary>
        /// Removes the module from web application properties.
        /// Intended only for SPWebApplication feature receiver.
        /// </summary>
        /// <typeparam name="T">type of the module</typeparam>
        /// <param name="webApplication">The web application.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        public static void RemoveModule<T>(SPWebApplication webApplication, bool recursive = false)
            where T : IModule
        {
            if(webApplication == null)
                throw new ArgumentNullException("webApplication");
            IPropertyProvider propertyProvider = new WebApplicationPropertyProvider(webApplication);
            RemoveModule<T>(propertyProvider, recursive);
        }

        /// <summary>
        /// Removes the module from the site properties.
        /// Intended only for SPSite feature receiver
        /// </summary>
        /// <typeparam name="T">type of the module</typeparam>
        /// <param name="site">The site.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        public static void RemoveModule<T>(SPSite site, bool recursive = false)
            where T : IModule
        {
            if (site == null)
                throw new ArgumentNullException("site");
            IPropertyProvider propertyProvider = new SitePropertyProvider(site);
            RemoveModule<T>(propertyProvider, recursive);
        }

        private static void RemoveModule<T>(IPropertyProvider propertyProvider, bool recursive = false)
        {
            string assemblyQualifiedName = typeof(T).AssemblyQualifiedName;
            IList<ModuleConfiguration> moduleConfigurations = Configuration.ReadAllModuleConfigurations(propertyProvider);

            if(moduleConfigurations.Count == 0)
            {
                Trace.WriteLine(string.Format("Could not remove module {0} from {1} as no moduleconfigurations could be found", assemblyQualifiedName, propertyProvider));
                return;
            }
            if(!moduleConfigurations.Any(mc => mc.Scope == propertyProvider.Scope))
            {
                Trace.WriteLine(string.Format("Could not remove module {0} from {1} as no moduleconfigurations could be found for that specific scope ({2})", assemblyQualifiedName, propertyProvider, Enum.GetName(typeof(SPScope), propertyProvider.Scope)));
                return;
            }

            // if recursive flag is not set, we only remove that module from the specified scope (Site, App, Farm)
            // and do not recursively remove it from all other scopes)
            if (!recursive)
                moduleConfigurations = moduleConfigurations.Take(1).ToList();

            foreach (ModuleConfiguration moduleConfiguration in moduleConfigurations)
            {
                var moduleConfigXml = moduleConfiguration.Configuration;
                if (moduleConfigXml.HasElements)
                {
                    XElement[] matchingModules = moduleConfigXml.XPathSelectElements("module[@type='" + assemblyQualifiedName + "']").ToArray();
                    if (matchingModules.Any())
                    {
                        Trace.WriteLine(string.Format("Module {0} removed from {1}", assemblyQualifiedName, propertyProvider.ToString()));
                        matchingModules.Remove();

                        Configuration.UpdatePersistedModules(moduleConfigXml, propertyProvider);
                    }
                }
            }
        }
    }
}