using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint
{
    public static class Configuration
    {
        private const string AUTOFAC_MOD_KEY = "_autofac_modules";
        private const string AUTOFAC_MOD_CHANGE_KEY = "_autofac_module_changedate";
        private const string AUTOFAC_MOD_CHANGE_FORMAT = "yyyy/MM/dd hh:mm:ss:FFFF";
        
        internal static IList<IModule> GetConfiguredModules()
        {
            var propertyProvider = PropertyProviderFactory.GetPropertyProvider();
            return ReadAllModules(propertyProvider);
        }

        internal static IList<IModule> GetConfiguredModules(SPSite site)
        {
            if (site == null)
                throw new ArgumentNullException("site");
            var propertyProvider = PropertyProviderFactory.GetPropertyProvider(site);
            return ReadAllModules(propertyProvider);
        }

        internal static IList<IModule> GetConfiguredModules(SPWebApplication webApplication)
        {
            if (webApplication == null)
                throw new ArgumentNullException("webApplication");
            var propertyProvider = PropertyProviderFactory.GetPropertyProvider(webApplication);
            return ReadAllModules(propertyProvider);
        }

        internal static IList<IModule> GetConfiguredModules(IPropertyProvider propertyProvider)
        {
            if(propertyProvider == null)
                throw new ArgumentNullException("propertyProvider");
            return ReadAllModules(propertyProvider);
        }

        internal static IList<ModuleConfiguration> ReadAllModuleConfigurations(IPropertyProvider propertyProvider)
        {
            return
                propertyProvider.GetAllPropertiesRecursive<ModuleConfiguration>((tmp) => ReadModuleConfiguration(tmp))
                    .Where(mc => mc != null)
                        .ToList();
        }

        internal static ModuleConfiguration ReadModuleConfiguration(IIndexAccessible propertyProvider)
        {
            string config = propertyProvider[AUTOFAC_MOD_KEY] as string;
            string configChange = propertyProvider[AUTOFAC_MOD_CHANGE_KEY] as string;

            if (string.IsNullOrEmpty(config))
                return null;

            DateTime configChangeTime = DateTime.MinValue;
            if(!string.IsNullOrEmpty(configChange))
                configChangeTime = DateTime.ParseExact(configChange, AUTOFAC_MOD_CHANGE_FORMAT, CultureInfo.InvariantCulture);
            
            return new ModuleConfiguration(config,
                                            propertyProvider.Scope,
                                            configChangeTime,
                                            propertyProvider.ID);
        }

        internal static IList<IModule> ReadAllModules(IPropertyProvider propertyProvider)
        {
            IList<ModuleConfiguration> moduleConfigurations = ReadAllModuleConfigurations(propertyProvider);
            return ReadAllModules(moduleConfigurations);
        }

        internal static IList<IModule> ReadAllModules(IList<ModuleConfiguration> moduleConfigurations)
        {
            List<IModule> modules = new List<IModule>();
            foreach (var moduleConfig in moduleConfigurations)
                modules.AddRange(DeserializeModules(moduleConfig.Configuration));
            return modules;
        }

        internal static void UpdatePersistedModules(XElement modules, IPropertyProvider propertyProvider)
        {
            if (modules == null)
                return;

            propertyProvider[AUTOFAC_MOD_KEY] = modules.ToString();
            propertyProvider[AUTOFAC_MOD_CHANGE_KEY] = DateTime.Now.ToString(AUTOFAC_MOD_CHANGE_FORMAT, CultureInfo.InvariantCulture);
            propertyProvider.Update();
        }

        private static IList<IModule> DeserializeModules(XElement xModules)
        {
            var iModules = new List<IModule>();
            foreach (XElement xModule in xModules.XPathSelectElements("module"))
            {
                var typeAttribute = xModule.Attribute(XName.Get("type", ""));
                if (typeAttribute == null)
                    continue;

                string typeName = typeAttribute.Value;
                IEnumerable<XElement> xParameters = xModule.XPathSelectElements("parameters/parameter");
                IEnumerable<XElement> xProperties = xModule.XPathSelectElements("properties/property");
                IEnumerable<Parameter> parameters = ToParameters(xParameters);
                IEnumerable<Parameter> properties = ToParameters(xProperties);

                Type type = Type.GetType(typeName);
                if (type != null)
                {
                    var module =
                        (IModule)
                        new ReflectionActivator(type, new BindingFlagsConstructorFinder(BindingFlags.Public),
                                                new MostParametersConstructorSelector(), parameters, properties).
                            ActivateInstance(Autofac.Core.Container.Empty, Enumerable.Empty<Parameter>());
                    iModules.Add(module);
                }
            }
            return iModules;
        }

        private static IEnumerable<Parameter> ToParameters(IEnumerable<XElement> elements)
        {
            return elements.Select(element =>
            {
                return
                    (Parameter)
                    new ResolvedParameter(
                        ((pi, c) => pi.Name == element.Attribute(XName.Get("name", "")).Value),
                        ((pi, c) =>
                         AlternateTypeManipulation.ChangeToCompatibleType(
                             element.Attribute(XName.Get("value", "")).Value,
                             pi.ParameterType)));
            }).ToList();
        }

        private class AlternateTypeManipulation
        {
            public static object ChangeToCompatibleType(object value, Type destinationType)
            {
                if (destinationType == null)
                    throw new ArgumentNullException("destinationType");
                if (value == null)
                {
                    if (destinationType.IsValueType)
                        return Activator.CreateInstance(destinationType);
                    else
                        return null;
                }
                else
                {
                    TypeConverter converter1 = TypeDescriptor.GetConverter(value.GetType());
                    if (converter1 != null && converter1.CanConvertTo(destinationType))
                        return converter1.ConvertTo(value, destinationType);
                    if (destinationType.IsAssignableFrom(value.GetType()))
                        return value;
                    TypeConverter converter2 = TypeDescriptor.GetConverter(destinationType);
                    if (converter2 == null)
                        throw new ConfigurationErrorsException(string.Format("Cannot convert type from {0} to {1}.",
                                                                             (object)value.GetType(),
                                                                             (object)destinationType));
                    else
                        return converter2.ConvertFrom(value);
                }
            }
        }
    }

    internal class ModuleConfiguration
    {
        private readonly string _configurationValue;
        private XElement _configuration = null;

        public ModuleConfiguration(string configurationValue, SPScope scope, DateTime modifiedAt, Guid id)
        {
            _configurationValue = configurationValue;
            this.Scope = scope;
            this.ModifiedAt = modifiedAt;
            this.ID = id;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public XElement Configuration
        {
            get
            {
                if (_configuration == null)
                    _configuration = TryParseConfig(_configurationValue);

                return _configuration;
            }
        }

        /// <summary>
        /// Gets the scope.
        /// </summary>
        public SPScope Scope { get; private set; }

        /// <summary>
        /// Gets the modified at date and time.
        /// </summary>
        public DateTime ModifiedAt { get; private set; }

        /// <summary>
        /// Gets the ID of the corresponging SPFarm, SPWebApplication, SPSite object
        /// </summary>
        public Guid ID { get; private set; }

        private static XElement TryParseConfig(string configString)
        {
            return XElement.Parse(string.IsNullOrEmpty(configString) ? "</modules>" : configString);
        }
    }
}
