﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Imaging;
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies
{
    [ProjectSystemTrait]
    public class DependencyNodeTests
    {
        [Fact]
        public void DependencyNode_Constructor_WhenIdIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>("id", () => {
                new DependencyNode(null, ProjectTreeFlags.Empty);
            });
        }

        [Theory]
        [InlineData("file:///[MyProviderType;MyItemSpec;MyItemType;MyUniqueToken]", "MyItemSpec")]
        [InlineData("file:///[MyProviderType]", "")]
        public void DependencyNode_Constructor_Caption(string idString, string expectedCaption)
        {
            // Arrange
            var id = DependencyNodeId.FromString(idString);
            var properties = new Dictionary<string, string>().ToImmutableDictionary();
            var priority = 2;

            // Act
            var node = new DependencyNode(id, ProjectTreeFlags.Empty, priority, properties);

            // Assert
            Assert.Equal(expectedCaption, node.Caption);
            Assert.Equal(priority, node.Priority);
            Assert.Equal(properties, node.Properties);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DependencyNode_Constructor_FlagsAndResolved(bool resolved)
        {
            // Arrange
            var id = DependencyNodeId.FromString(
                        "file:///[MyProviderType;MyItemSpec;MyItemType;MyUniqueToken]");
            var myFlags = ProjectTreeFlags.Create("CustomFlag");

            // Act
            var node = new DependencyNode(id, myFlags, resolved: resolved);

            // Assert
            var expectedFlags = resolved
                ? DependencyNode.GenericResolvedDependencyFlags.Union(myFlags)
                : DependencyNode.GenericUnresolvedDependencyFlags.Union(myFlags);

            Assert.True(node.Flags.Contains(expectedFlags));
            Assert.Equal(resolved, node.Resolved);
        }

        [Theory]
        [InlineData("file:///[MyProviderType;MyItemSpec;MyItemType;MyUniqueToken]", "MyItemSpec", "MyItemSpec")]
        [InlineData("file:///[MyProviderType;MyItemSpec;MyItemType;MyUniqueToken]", 
                    "OtherCaption", 
                    "OtherCaption (MyItemSpec)")]
        public void DependencyNode_Alias(string idString, string caption, string expectedAlias)
        {
            // Arrange
            var id = DependencyNodeId.FromString(idString);

            // Act
            var node = new DependencyNode(id, ProjectTreeFlags.Empty);
            node.Caption = caption;

            // Assert
            Assert.Equal(expectedAlias, node.Alias);
        }

        [Theory]
        [InlineData("file:///[MyProviderType;MyItemSpec;MyItemType;MyUniqueToken]",
                    "file:///[myprovidertype;myitemspec;myitemtype;myuniquetoken]",
                    true)]
        [InlineData("file:///[MyProviderType;MyItemSpec;MyItemType;MyUniqueToken]",
                    "file:///[myprovidertype;myitemspec;myitemtype]",
                     false)]
        public void DependencyNode_Equals(string firstIdString,
                                          string secondIdString,
                                          bool expectedResult)
        {
            // Arrange
            var id1 = DependencyNodeId.FromString(firstIdString);
            var id2 = DependencyNodeId.FromString(secondIdString);

            // Act
            var node1 = new DependencyNode(id1, ProjectTreeFlags.Empty);
            var node2 = new DependencyNode(id2, ProjectTreeFlags.Empty);

            // Assert
            Assert.Equal(expectedResult, node1.Equals(node2));
            Assert.Equal(expectedResult, node2.Equals(node1));
        }

        [Fact]
        public void DependencyNode_Children()
        {
           var id = DependencyNodeId.FromString(
                "file:///[MyProviderType;MyItemSpec;MyItemType;MyUniqueToken]");
            
            var node = new DependencyNode(id, ProjectTreeFlags.Empty);

            // check empty node
            Assert.False(node.HasChildren);

            // add new node
            var childId = DependencyNodeId.FromString(
                 "file:///[MyProviderType;MyItemSpec2;MyItemType2;MyUniqueToken2]");
            var childNode = new DependencyNode(childId, ProjectTreeFlags.Empty);

            node.AddChild(childNode);
            Assert.True(node.HasChildren);

            // remove child node
            node.RemoveChild(childNode);
            Assert.False(node.HasChildren);
        }

        [Theory]
        [InlineData("MyFusionName", "MyFusionName")]
        [InlineData("", "MyItemSpec")]
        public void AssemblyDependencyNode_Constructor_ResolvedUnresolved(
                            string fusionName,
                            string expectedCaption)
        {
            // Arrange
            var priority = 3;
            var id = DependencyNodeId.FromString(
                        "file:///[MyProviderType;c:\\MyItemSpec.dll;MyItemType;MyUniqueToken]");
            var properties = new Dictionary<string, string>().ToImmutableDictionary();

            // Act
            var node = new AssemblyDependencyNode(id, 
                                                  ProjectTreeFlags.Empty, 
                                                  priority, 
                                                  properties,
                                                  resolved: true,
                                                  fusionName: fusionName);

            // Assert
            Assert.Equal(KnownMonikers.Reference, node.Icon);
            Assert.Equal(true, node.Resolved);
            Assert.Equal(expectedCaption, node.Caption);

            // Just to double-check that these properties are still set as sexpected
            Assert.Equal(priority, node.Priority);
            Assert.Equal(properties, node.Properties);
            Assert.Equal(node.Icon, node.ExpandedIcon);
        }

        [Theory]
        [InlineData("MyFusionName")]
        [InlineData("")]
        public void AssemblyDependencyNode_Constructor_Resolved(
                            string fusionName)
        {
            // Arrange
            var priority = 3;
            var id = DependencyNodeId.FromString(
                        "file:///[MyProviderType;c:\\MyItemSpec.dll;MyItemType;MyUniqueToken]");
            var properties = new Dictionary<string, string>().ToImmutableDictionary();

            // Act
            var node = new AssemblyDependencyNode(id,
                                                  ProjectTreeFlags.Empty,
                                                  priority,
                                                  properties,
                                                  resolved: false,
                                                  fusionName: fusionName);

            // Assert
            Assert.Equal(KnownMonikers.ReferenceWarning, node.Icon);
            Assert.Equal(false, node.Resolved);
            Assert.Equal("MyItemSpec.dll", node.Caption);

            // Just to double-check that these properties are still set as sexpected
            Assert.Equal(priority, node.Priority);
            Assert.Equal(properties, node.Properties);
            Assert.Equal(node.Icon, node.ExpandedIcon);
        }

        [Theory]
        [InlineData(false, "MyItemSpec.dll")]
        [InlineData(true, "MyItemSpec")]
        public void ComDependencyNode_Constructor(bool resolved, string expectedCaption)
        {
            // Arrange
            var expectedIcon = resolved
                ? KnownMonikers.Component
                : KnownMonikers.ReferenceWarning;
            var priority = 3;
            var id = DependencyNodeId.FromString(
                        "file:///[MyProviderType;c:\\MyItemSpec.dll;MyItemType;MyUniqueToken]");
            var properties = new Dictionary<string, string>().ToImmutableDictionary();

            // Act
            var node = new ComDependencyNode(id,
                                             ProjectTreeFlags.Empty,
                                             priority,
                                             properties,
                                             resolved: resolved);

            // Assert
            Assert.Equal(expectedIcon, node.Icon);
            Assert.Equal(resolved, node.Resolved);
            Assert.Equal(expectedCaption, node.Caption);

            // Just to double-check that these properties are still set as sexpected
            Assert.Equal(priority, node.Priority);
            Assert.Equal(properties, node.Properties);
            Assert.Equal(node.Icon, node.ExpandedIcon);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SdkDependencyNode_Constructor(bool resolved)
        {
            // Arrange
            var expectedIcon = resolved
                ? KnownMonikers.BrowserSDK
                : KnownMonikers.ReferenceWarning;
            var priority = 3;
            var id = DependencyNodeId.FromString(
                        "file:///[MyProviderType;c:\\MyItemSpec.dll;MyItemType;MyUniqueToken]");
            var properties = new Dictionary<string, string>().ToImmutableDictionary();

            // Act
            var node = new SdkDependencyNode(id,
                                             ProjectTreeFlags.Empty,
                                             priority,
                                             properties,
                                             resolved: resolved);

            // Assert
            Assert.Equal(expectedIcon, node.Icon);
            Assert.Equal(resolved, node.Resolved);
            Assert.Equal("c:\\MyItemSpec.dll", node.Caption);

            // Just to double-check that these properties are still set as sexpected
            Assert.Equal(priority, node.Priority);
            Assert.Equal(properties, node.Properties);
            Assert.Equal(node.Icon, node.ExpandedIcon);
        }

        [Fact]
        public void SdkDependencyNode_Constructor_WhenItemSpecIsNullOrEmpty_ShouldThrow()
        {
            var id = DependencyNodeId.FromString("file:///[MyProviderType]");

            Assert.Throws<ArgumentException>("ItemSpec", () => {
                new SdkDependencyNode(id, ProjectTreeFlags.Empty);
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SharedProjectDependencyNode_Constructor(bool resolved)
        {
            // Arrange
            var expectedIcon = resolved
                ? KnownMonikers.SharedProject
                : KnownMonikers.ReferenceWarning;
            var priority = 3;
            var id = DependencyNodeId.FromString(
                        "file:///[MyProviderType;c:\\MyItemSpec.dll;MyItemType;MyUniqueToken]");
            var properties = new Dictionary<string, string>().ToImmutableDictionary();

            // Act
            var node = new SharedProjectDependencyNode(id,
                                                       ProjectTreeFlags.Empty,
                                                       priority,
                                                       properties,
                                                       resolved: resolved);

            // Assert
            Assert.Equal(expectedIcon, node.Icon);
            Assert.Equal(resolved, node.Resolved);
            Assert.Equal("MyItemSpec", node.Caption);

            // Just to double-check that these properties are still set as sexpected
            Assert.Equal(priority, node.Priority);
            Assert.Equal(properties, node.Properties);
            Assert.Equal(node.Icon, node.ExpandedIcon);
        }

        [Fact]
        public void SharedProjectDependencyNode_Constructor_WhenItemSpecIsNullOrEmpty_ShouldThrow()
        {
            var id = DependencyNodeId.FromString("file:///[MyProviderType]");

            Assert.Throws<ArgumentException>("ItemSpec", () => {
                new SharedProjectDependencyNode(id, ProjectTreeFlags.Empty);
            });
        }
    }
}
