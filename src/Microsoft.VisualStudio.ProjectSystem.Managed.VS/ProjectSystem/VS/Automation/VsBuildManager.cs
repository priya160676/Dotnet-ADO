﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem.VS.ConnectionPoint;
using VSLangProj;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Automation
{
    /// <summary>
    /// Manages the portable executable (PE) files produced by running custom tools.
    /// </summary>
    [Export(typeof(BuildManager))]
    [AppliesTo(ProjectCapability.CSharpOrVisualBasic)]
    [Order(Order.Default)]
    internal class VSBuildManager : ConnectionPointContainer,
                                    IEventSource<_dispBuildManagerEvents>,
                                    BuildManager,
                                    BuildManagerEvents
    {
        private readonly VSLangProj.VSProject _vsProject;

        private readonly IUnconfiguredProjectCommonServices _unconfiguredProjectServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSBuildManager"/> class.
        /// </summary>
        [ImportingConstructor]
        internal VSBuildManager(
            [Import(ExportContractNames.VsTypes.CpsVSProject)] VSLangProj.VSProject vsProject,
            IUnconfiguredProjectCommonServices unconfiguredProjectServices)
        {
            AddEventSource(this as IEventSource<_dispBuildManagerEvents>);
            _vsProject = vsProject;
            _unconfiguredProjectServices = unconfiguredProjectServices;
        }

        #region _dispBuildManagerEvents_Event Members

        /// <summary>
        /// Occurs when a design time output moniker is deleted.
        /// </summary>
        public event _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler DesignTimeOutputDeleted;

        /// <summary>
        /// Occurs when a design time output moniker is dirty.
        /// </summary>
        public event _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler DesignTimeOutputDirty;

        #endregion

        /// <summary>
        /// Gets the project of which the selected item is a part.
        /// </summary>
        public EnvDTE.Project ContainingProject
        {
            get { return _vsProject.Project; }
        }

        /// <summary>
        /// Gets the top-level extensibility object.
        /// </summary>
        public EnvDTE.DTE DTE
        {
            get { return _vsProject.DTE; }
        }

        /// <summary>
        /// Gets the immediate parent object of a given object.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public object Parent
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the temporary portable executable (PE) monikers for a project.
        /// </summary>
        public object DesignTimeOutputMonikers
        {
            get
            {
                var project = _unconfiguredProjectServices.ActiveConfiguredProject;
                var ruleSource = project.Services.ProjectSubscription.ProjectRuleSource;
                var update = _unconfiguredProjectServices.ThreadingService.ExecuteSynchronously(() => ruleSource.GetLatestVersionAsync(project, new string[] { Compile.SchemaName }));
                var snapshot = update[Compile.SchemaName];

                var monikers = new List<string>();
                foreach (var item in snapshot.Items.Values)
                {
                    bool isLink = GetBooleanPropertyValue(item, Compile.LinkProperty);
                    bool designTime = GetBooleanPropertyValue(item, Compile.DesignTimeProperty);

                    if (!isLink && designTime)
                    {
                        if (item.TryGetValue(Compile.FullPathProperty, out string path))
                        {
                            monikers.Add(path);
                        }
                    }
                }
                
                return monikers.ToArray();

                bool GetBooleanPropertyValue(System.Collections.Immutable.IImmutableDictionary<string, string> item, string propertyName)
                {
                    return item.TryGetValue(propertyName, out string value) && StringComparers.PropertyValues.Equals(value, "true");
                }
            }
        }

        #region BuildManager Members

        /// <summary>
        /// Builds a temporary portable executable (PE) and returns its description in an XML string.
        /// </summary>
        public string BuildDesignTimeOutput(string bstrOutputMoniker)
        {
            var languageName = _unconfiguredProjectServices.ThreadingService.ExecuteSynchronously(async () =>
            {
                var property = await _unconfiguredProjectServices.ActiveConfiguredProjectProperties.GetConfigurationGeneralPropertiesAsync();
                return await property.LanguageServiceName.GetValueAsync();
            });

            // TODO: Once Roslyn work is complete:
            //
            //   var compiler = _serviceProvider.GetService(typeof(Microsoft.VisualStudio.LanguageServices.Implementation.ProjectSystem.CPS.ITempPECompilerHost));
            //  (after importing a service provider in the constructor as: [Import(typeof(SVsServiceProvider))]IServiceProvider serviceProvider)
            //  (or maybe can just import a Lazy<ITempPECompilerHost> in the constructor?)
            //
            //   compiler.Compile(languageName ... );

            throw new NotImplementedException();
        }

        #endregion

        #region IEventSource<_dispBuildManagerEvents> Members

        void IEventSource<_dispBuildManagerEvents>.OnSinkAdded(_dispBuildManagerEvents sink)
        {
            DesignTimeOutputDeleted += new _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler(sink.DesignTimeOutputDeleted);
            DesignTimeOutputDirty += new _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler(sink.DesignTimeOutputDirty);
        }

        void IEventSource<_dispBuildManagerEvents>.OnSinkRemoved(_dispBuildManagerEvents sink)
        {
            DesignTimeOutputDeleted -= new _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler(sink.DesignTimeOutputDeleted);
            DesignTimeOutputDirty -= new _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler(sink.DesignTimeOutputDirty);
        }

        #endregion

        /// <summary>
        /// Occurs when a design time output moniker is deleted.
        /// </summary>
        protected void OnDesignTimeOutputDeleted(string outputMoniker)
        {
            DesignTimeOutputDeleted?.Invoke(outputMoniker);
        }

        /// <summary>
        /// Occurs when a design time output moniker is dirty.
        /// </summary>
        protected void OnDesignTimeOutputDirty(string outputMoniker)
        {
            DesignTimeOutputDirty?.Invoke(outputMoniker);
        }
    }
}
