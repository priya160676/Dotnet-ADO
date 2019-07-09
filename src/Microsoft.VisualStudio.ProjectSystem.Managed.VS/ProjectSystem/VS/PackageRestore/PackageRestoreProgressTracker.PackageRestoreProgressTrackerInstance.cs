﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.ProjectSystem.OperationProgress;

namespace Microsoft.VisualStudio.ProjectSystem.VS.PackageRestore
{
    internal partial class PackageRestoreProgressTracker
    {
        internal class PackageRestoreProgressTrackerInstance : OnceInitializedOnceDisposed, IMultiLifetimeInstance
        {
            // The steps leading up to and during restore are the following:
            //
            //      1) Evaluation
            //      2) Design-Time build (call CollectPackageReferences, et all)
            //      3) Push package references & restore data to NuGet ("Nominate")
            //      4) If assets file updated during restore, repeat above
            //
            // It can take two cycles of above (during first open, or when assets file is out of 
            // date) before we have a design-time build that contains all the references that a 
            // project depends on, up to and including mscorlib/System.Runtime.
            // 
            // We want the "IntelliSense" operation progress stage to only be considered completed
            // once we've stopped this cycle, which will let Roslyn, designers and other consumers
            // disable commands, or give indicators that the project is still loading.
            //
            // To figure out when we've finished the cycle, we compare the last write time of the 
            // assets file during the last evaluation against the timestamp of the file on disk. 
            // If they don't match, we know that we're about to repeat and we're still incomplete. 
            // Once they match in timestamp, we know that the last design-time build ran with the 
            // new file and we notify operation progress that we've completed with these results.

            private readonly ConfiguredProject _project;
            private readonly IDataProgressTrackerService _dataProgressTrackerService;
            private readonly IPackageRestoreService _restoreService;
            private readonly IProjectSubscriptionService _projectSubscriptionService;

            private IDataProgressTrackerServiceRegistration? _progressRegistration;
            private IDisposable? _subscription;

            public PackageRestoreProgressTrackerInstance(
                ConfiguredProject project,
                IDataProgressTrackerService dataProgressTrackerService,
                IPackageRestoreService restoreService,
                IProjectSubscriptionService projectSubscriptionService)
            {
                _project = project;
                _dataProgressTrackerService = dataProgressTrackerService;
                _restoreService = restoreService;
                _projectSubscriptionService = projectSubscriptionService;
            }

            public Task InitializeAsync()
            {
                EnsureInitialized();

                return Task.CompletedTask;
            }

            protected override void Initialize()
            {
                _progressRegistration = _dataProgressTrackerService.RegisterForIntelliSense(
                    _project,
                    nameof(PackageRestoreProgressTracker));

                Action<IProjectVersionedValue<Tuple<RestoreData, IProjectSnapshot>>> action = OnRestoreCompleted;

                _subscription = ProjectDataSources.SyncLinkTo(
                        _restoreService.RestoreData.SyncLinkOptions(),
                        _projectSubscriptionService.ProjectSource.SourceBlock.SyncLinkOptions(),
                        DataflowBlockSlim.CreateActionBlock(action),
                        linkOptions: DataflowOption.PropagateCompletion);
            }

            private void OnRestoreCompleted(IProjectVersionedValue<Tuple<RestoreData, IProjectSnapshot>> value)
            {
                if (IsRestoreUpToDate(value.Value.Item1, value.Value.Item2))
                {
                    _progressRegistration!.NotifyOutputDataCalculated(value.DataSourceVersions);
                }
            }

            private static bool IsRestoreUpToDate(RestoreData restoreData, IProjectSnapshot projectSnapshot)
            {
                DateTime lastEvaluationWriteTime = GetLastWriteTimeUtc(restoreData.ProjectAssetsFilePath, projectSnapshot);

                return lastEvaluationWriteTime == restoreData.ProjectAssetsLastWriteTimeUtc;
            }
            
            private static DateTime GetLastWriteTimeUtc(string filePath, IProjectSnapshot projectSnapshot)
            {
                if (projectSnapshot is IProjectSnapshot2 projectSnapshot2 &&
                    projectSnapshot2.AdditionalDependentFileTimes.TryGetValue(filePath, out DateTime lastWriteTimeUtc))
                {
                    return lastWriteTimeUtc;
                }

                // No assets file in the project, or the file wasn't included 
                // as part of the <AdditionalDesignTimeBuildInput> item
                return DateTime.MinValue;
            }

            protected override void Dispose(bool disposing)
            {
                _progressRegistration?.Dispose();
                _subscription?.Dispose();
            }
        }
    }
}
