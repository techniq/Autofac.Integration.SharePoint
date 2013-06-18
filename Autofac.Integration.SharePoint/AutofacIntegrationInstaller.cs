using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Integration.SharePoint.Forms;
using Microsoft.SharePoint.Administration;

namespace Autofac.Integration.SharePoint
{
    public class AutofacIntegrationInstaller
    {
        public static void Install(SPWebApplication webApp)
        {
            foreach (var entry in AutofacConfigEntries)
            {
                webApp.WebConfigModifications.Add(entry.Prepare());
            }

            // set flag specifying, that DI of autofac is enabled on that site
            webApp.Properties[Constants.AUTOFAC_DI_ENABLED] = true;
            webApp.Update();
        }

        public static void Uninstall(SPWebApplication webApp)
        {
            var modsCollection = webApp.WebConfigModifications;

            for (var i = modsCollection.Count - 1; i > -1; i--)
            {
                if (modsCollection[i].Owner == ConfigModsOwnerName)
                {
                    // Remove it and save the change to the configuration database  
                    modsCollection.Remove(modsCollection[i]);
                }
            }

            // remove flag
            if (webApp.Properties.ContainsKey(Constants.AUTOFAC_DI_ENABLED))
                webApp.Properties.Remove(Constants.AUTOFAC_DI_ENABLED);

            webApp.Update();
        }

        #region Web.Config Modification Entries
        private const string ConfigModsOwnerName = "Autofac Integration For SharePoint";
        private static readonly WebConfigEntry[] AutofacConfigEntries = 
            { 
                // configSections entry
                new WebConfigEntry( 
                    "section[@name='autofac']" 
                    ,"configuration/configSections" 
                    ,"<section name=\"autofac\" type=\"" + typeof(Autofac.Configuration.SectionHandler).AssemblyQualifiedName + "\"/>" 
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)  

                // autofac entry
                ,new WebConfigEntry(
                    "autofac" 
                    ,"configuration" 
                    ,"<autofac/>" 
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)   

                // autofac modules entry
                ,new WebConfigEntry(
                    "modules" 
                    ,"configuration/autofac" 
                    ,"<modules/>" 
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                //// This is just a sample module
                //,new WebConfigEntry(
                //    "module[@type='" + typeof(Playground.PlaygroundModule).AssemblyQualifiedName + "']"
                //    ,"configuration/autofac/modules"
                //    ,"<module type=\"" + typeof(Playground.PlaygroundModule).AssemblyQualifiedName + "\"/>"
                //    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                //    , false)

                // autofac httpModules entries
                ,new WebConfigEntry( 
                    "add[@name='ServiceLocatorInitialization']" 
                    ,"configuration/system.web/httpModules" 
                    ,"<add name=\"ServiceLocatorInitialization\" type=\"" + typeof(ServiceLocatorInitializationModule).AssemblyQualifiedName + "\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                ,new WebConfigEntry( 
                    "add[@name='ContainerDisposal']" 
                    ,"configuration/system.web/httpModules" 
                    ,"<add name=\"ContainerDisposal\" type=\"" + typeof(ContainerDisposalModule).AssemblyQualifiedName + "\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false) 
                ,new WebConfigEntry( 
                    "add[@name='PropertyInjection']" 
                    ,"configuration/system.web/httpModules" 
                    ,"<add name=\"PropertyInjection\" type=\"" + typeof(PropertyInjectionModule).AssemblyQualifiedName + "\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                ,new WebConfigEntry( 
                    "add[@name='AttributeInjection']" 
                    ,"configuration/system.web/httpModules" 
                    ,"<add name=\"AttributeInjection\" type=\"" + typeof(AttributedInjectionModule).AssemblyQualifiedName + "\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                    
                // autofac iis7 modules entries
                ,new WebConfigEntry( 
                    "add[@name='ServiceLocatorInitialization']" 
                    ,"configuration/system.webServer/modules" 
                    ,"<add name=\"ServiceLocatorInitialization\" type=\"" + typeof(ServiceLocatorInitializationModule).AssemblyQualifiedName + "\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                ,new WebConfigEntry( 
                    "add[@name='ContainerDisposal']" 
                    ,"configuration/system.webServer/modules" 
                    ,"<add name=\"ContainerDisposal\" type=\"" + typeof(ContainerDisposalModule).AssemblyQualifiedName + "\" preCondition=\"managedHandler\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false) 
                ,new WebConfigEntry( 
                    "add[@name='PropertyInjection']" 
                    ,"configuration/system.webServer/modules" 
                    ,"<add name=\"PropertyInjection\" type=\"" + typeof(PropertyInjectionModule).AssemblyQualifiedName + "\" preCondition=\"managedHandler\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                ,new WebConfigEntry( 
                    "add[@name='AttributeInjection']" 
                    ,"configuration/system.webServer/modules" 
                    ,"<add name=\"AttributeInjection\" type=\"" + typeof(AttributedInjectionModule).AssemblyQualifiedName + "\" preCondition=\"managedHandler\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                
                // autofac assemblies entries
                ,new WebConfigEntry( 
                    "add[@assembly='" + typeof(Autofac.IContainer).Assembly.FullName + "']" 
                    ,"configuration/system.web/compilation/assemblies" 
                    ,"<add assembly=\"" + typeof(Autofac.IContainer).Assembly.FullName + "\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                ,new WebConfigEntry( 
                    "add[@assembly='" + typeof(Autofac.Configuration.ComponentElement).Assembly.FullName + "']" 
                    ,"configuration/system.web/compilation/assemblies" 
                    ,"<add assembly=\"" + typeof(Autofac.Configuration.ComponentElement).Assembly.FullName + "\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                ,new WebConfigEntry( 
                    "add[@assembly='" + typeof(IContainerProviderAccessor).Assembly.FullName + "']" 
                    ,"configuration/system.web/compilation/assemblies" 
                    ,"<add assembly=\"" + typeof(IContainerProviderAccessor).Assembly.FullName + "\" />"
                    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                    ,false)
                    
                //// add some safe control entries as well (assuming you'll be building some controls in Autofac.Integration.SharePoint)
                //,new WebConfigEntry( 
                //    "SafeControl[@Assembly='" + typeof(Autofac.Integration.SharePoint.GlobalApplication).Assembly.FullName + "']"
                //    ,"configuration/SharePoint/SafeControls" 
                //    ,"<SafeControl Assembly=\"" + typeof(Autofac.Integration.SharePoint.GlobalApplication).Assembly.FullName + "\" Namespace=\"Autofac.Integration.SharePoint\" TypeName=\"*\" Safe=\"True\" SafeAgainstScript=\"False\" />"
                //    ,SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode 
                //    ,false)

            };

        /// <summary> 
        /// Container to hold info about our modifications to the web.config. 
        /// </summary> 
        public class WebConfigEntry
        {
            public string Name;
            public string XPath;
            public string Value;
            public SPWebConfigModification.SPWebConfigModificationType ModificationType;
            public bool KeepOnDeactivate;

            public WebConfigEntry(string name, string xPath, string value,
                SPWebConfigModification.SPWebConfigModificationType modificationType, bool keepOnDeactivate)
            {
                Name = name;
                XPath = xPath;
                Value = value;
                ModificationType = modificationType;
                KeepOnDeactivate = keepOnDeactivate;
            }

            public SPWebConfigModification Prepare()
            {
                var modification = new SPWebConfigModification(Name, XPath)
                {
                    Owner = ConfigModsOwnerName,
                    Sequence = 0,
                    Type = ModificationType,
                    Value = Value
                };
                return modification;
            }
        }
        #endregion
    }
}
