﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.VisualStudio.Input;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.Shell;

using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Input.Commands
{
    /// <summary>
    /// Updates the text of the Frameworks menu to include the current active framework. Instead of just saying
    /// Frameworks it will say Frameworks (netcoreapp1.0).
    /// </summary>
    [Export(typeof(DebugFrameworkPropertyMenuTextUpdater))]
    internal class DebugFrameworkPropertyMenuTextUpdater : OleMenuCommand
    {
        [ImportingConstructor]
        public DebugFrameworkPropertyMenuTextUpdater(IStartupProjectHelper startupProjectHelper)
                : base(ExecHandler, delegate
                { }, QueryStatusHandler,
                      new CommandID(new Guid(CommandGroup.ManagedProjectSystem), ManagedProjectSystemCommandId.DebugTargetMenuDebugFrameworkMenu))
        {
            StartupProjectHelper = startupProjectHelper;
        }

        private IStartupProjectHelper StartupProjectHelper { get; }

        /// <summary>
        /// Exec handler called when one of the menu items is selected. Does some
        /// basic validation before calling the commands QueryStatusCommand to update
        /// its state
        /// </summary>
        public static void ExecHandler(object sender, EventArgs e)
        {
            return;
        }


        /// <summary>
        /// QueryStatus handler called to update the status of the menu items. Does some
        /// basic validation before calling the commands QueryStatusCommand to update
        /// its state
        /// </summary>
        public static void QueryStatusHandler(object sender, EventArgs e)
        {
            if (!(sender is DebugFrameworkPropertyMenuTextUpdater command))
            {
                return;
            }

            command.QueryStatus();
        }

        public void QueryStatus()
        {
            ImmutableArray<IActiveDebugFrameworkServices> activeDebugFrameworks = StartupProjectHelper.GetExportFromDotNetStartupProjects<IActiveDebugFrameworkServices>(ProjectCapability.LaunchProfiles);
            if (activeDebugFrameworks.Length > 0)
            {
                string? activeFramework = null;
                List<string>? frameworks = null;
                ExecuteSynchronously(async () =>
                {
                    List<string>? first = null;

                    foreach (IActiveDebugFrameworkServices activeDebugFramework in activeDebugFrameworks)
                    {
                        frameworks = await activeDebugFramework.GetProjectFrameworksAsync();

                        if (first == null)
                        {
                            first = frameworks;
                        }
                        else
                        {
                            if (!first.SequenceEqual(frameworks))
                            {
                                frameworks = null;
                                break;
                            }
                        }
                    }

                    if (frameworks != null && frameworks.Count > 1)
                    {
                        // Only get this if we will need it down below
                        activeFramework = await activeDebugFrameworks[0].GetActiveDebuggingFrameworkPropertyAsync();
                    }
                });

                if (frameworks != null && frameworks.Count > 1)
                {
                    // If no active framework or the current active property doesn't match any of the frameworks, then
                    // set it to the first one.
                    if (!string.IsNullOrEmpty(activeFramework) && frameworks.Contains(activeFramework))
                    {
                        Text = string.Format(VSResources.DebugFrameworkMenuText, activeFramework);
                    }
                    else
                    {
                        Text = string.Format(VSResources.DebugFrameworkMenuText, frameworks[0]);
                    }

                    Visible = true;
                    Enabled = true;
                }
            }
        }

        /// <summary>
        /// For unit testing to wrap the JTF.Run call.
        /// </summary>
        protected virtual void ExecuteSynchronously(Func<Task> asyncFunction)
        {
#pragma warning disable VSTHRD102 // Only wrapped for test purposes
            ThreadHelper.JoinableTaskFactory.Run(asyncFunction);
#pragma warning restore VSTHRD102
        }
    }
}
